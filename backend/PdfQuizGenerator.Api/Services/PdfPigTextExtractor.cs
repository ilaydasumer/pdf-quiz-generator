using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace PdfQuizGenerator.Api.Services;

public class PdfPigTextExtractor : IPdfTextExtractor
{
    public string ExtractText(Stream pdfStream)
    {
        if (pdfStream == null || pdfStream.Length == 0)
        {
            throw new ArgumentException("The PDF file stream is empty or invalid.");
        }

        var textBuilder = new StringBuilder();

        try
        {
            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    // Use GetWords to ensure spaces between words are preserved,
                    // solving the "networkssuch" issue.
                    var words = page.GetWords().Select(w => w.Text);
                    var pageText = string.Join(" ", words);
                    textBuilder.AppendLine(pageText);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse the PDF document: {ex.Message}", ex);
        }

        var extractedText = textBuilder.ToString();

        // 1. Normalize line breaks and extra whitespace
        extractedText = Regex.Replace(extractedText, @"\s+", " ");

        // 2. Remove page numbers like 1, 2 at sentence endings (e.g. "word. 12 ")
        extractedText = Regex.Replace(extractedText, @"(?<=[a-zA-Z\.])\s+\d+\s*(?=[A-Z]|$)", " ");

        // 3. Fix missing spaces between lowercase and uppercase letters
        extractedText = Regex.Replace(extractedText, @"([a-z])([A-Z])", "$1 $2");

        // 4. General trim
        extractedText = extractedText.Trim();

        // Perform validation on the extracted text
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new ArgumentException("The uploaded PDF does not contain any readable text. It might be scanned or image-based.");
        }

        if (extractedText.Length < 100)
        {
            throw new ArgumentException("The extracted text is too short to generate a meaningful quiz. Please upload a PDF with more content.");
        }

        return extractedText;
    }
}
