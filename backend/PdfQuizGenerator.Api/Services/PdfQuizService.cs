using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PdfQuizGenerator.Api.Models;

namespace PdfQuizGenerator.Api.Services;

public class PdfQuizService : IQuizService
{
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;

    public PdfQuizService(IPdfTextExtractor pdfTextExtractor, HttpClient httpClient, IConfiguration configuration)
    {
        _pdfTextExtractor = pdfTextExtractor ?? throw new ArgumentNullException(nameof(pdfTextExtractor));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        // Check environment variable first, fallback to appsettings if not found
        _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                        ?? configuration["AiSettings:GeminiApiKey"] 
                        ?? string.Empty;
    }

    public async Task<List<QuizQuestion>> GenerateQuizAsync(Stream pdfStream, int questionCount, string difficulty)
    {
        var extractedText = _pdfTextExtractor.ExtractText(pdfStream);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new ArgumentException("Could not extract any text from the provided PDF.");
        }

        // Limit extracted text to avoid exceeding token limits
        if (extractedText.Length > 100000)
        {
            extractedText = extractedText.Substring(0, 100000);
        }

        int count = Math.Clamp(questionCount, 5, 15);
        if (string.IsNullOrWhiteSpace(difficulty)) difficulty = "Medium";

        if (string.IsNullOrEmpty(_geminiApiKey) || _geminiApiKey == "YOUR_GEMINI_API_KEY")
        {
            throw new Exception("Gemini API Key is not configured. Please add it to appsettings.json.");
        }

        var prompt = $@"
You are an expert educational quiz generator.
I will provide you with a text extracted from a document.
Please generate exactly {count} multiple-choice questions based on the text.
The difficulty of the questions should be: {difficulty}.

Requirements:
1. Return ONLY a valid JSON array of objects. Do not include markdown formatting (like ```json).
2. Each object must have the following exact structure:
{{
  ""questionText"": ""The question string"",
  ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
  ""correctAnswerIndex"": 0 // (0-3) integer index of the correct option
}}
3. Ensure there are exactly 4 options per question.
4. The JSON must be parseable.

Text:
{extractedText}
";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to generate quiz from AI. Status: {response.StatusCode}. Error: {errorContent}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(jsonResponse);
        var root = document.RootElement;
        
        var generatedText = root
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(generatedText))
        {
            throw new Exception("AI returned empty response.");
        }

        var cleanJson = generatedText.Trim();
        if (cleanJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleanJson = cleanJson.Substring(7);
        }
        if (cleanJson.EndsWith("```"))
        {
            cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
        }
        cleanJson = cleanJson.Trim();

        var aiQuestions = JsonSerializer.Deserialize<List<AiQuizQuestion>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (aiQuestions == null || !aiQuestions.Any())
        {
            throw new Exception("Could not parse AI response into quiz questions.");
        }

        var result = new List<QuizQuestion>();
        int idCounter = 1;
        foreach (var aiQ in aiQuestions.Take(count))
        {
            result.Add(new QuizQuestion
            {
                Id = idCounter++,
                QuestionText = aiQ.QuestionText ?? "Invalid Question",
                Options = aiQ.Options ?? new List<string> { "A", "B", "C", "D" },
                CorrectAnswerIndex = aiQ.CorrectAnswerIndex
            });
        }

        return result;
    }

    private class AiQuizQuestion
    {
        public string? QuestionText { get; set; }
        public List<string>? Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
    }
}
