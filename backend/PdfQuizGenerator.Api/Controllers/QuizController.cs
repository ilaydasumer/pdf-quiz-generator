using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PdfQuizGenerator.Api.DTOs;
using PdfQuizGenerator.Api.Services;

namespace PdfQuizGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(List<QuizQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateQuiz([FromForm] QuizGenerateRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("PDF file is required and cannot be empty.");
        }

        // Validate file extension
        var extension = System.IO.Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            return BadRequest("Only PDF files are supported.");
        }

        if (request.QuestionCount != 5 && request.QuestionCount != 10 && request.QuestionCount != 15)
        {
            return BadRequest("Question count must be 5, 10, or 15.");
        }

        try
        {
            // Call service
            var questions = await _quizService.GenerateQuizAsync(request.File.FileName, request.QuestionCount);

            // Map models to response DTOs
            var response = questions.Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Options = q.Options.Select((text, index) => new QuizOptionDto
                {
                    Index = index,
                    Text = text
                }).ToList(),
                CorrectAnswerIndex = q.CorrectAnswerIndex
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during quiz generation: {ex.Message}");
        }
    }
}
