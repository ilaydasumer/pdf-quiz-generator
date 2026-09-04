using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PdfQuizGenerator.Api.DTOs;
using PdfQuizGenerator.Api.Services;
using PdfQuizGenerator.Api.Data;
using PdfQuizGenerator.Api.Models.Entities;

namespace PdfQuizGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly AppDbContext _context;

    public QuizController(IQuizService quizService, AppDbContext context)
    {
        _quizService = quizService;
        _context = context;
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
            // Call service with file stream
            using var stream = request.File.OpenReadStream();
            var questions = await _quizService.GenerateQuizAsync(stream, request.QuestionCount, request.Difficulty);

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
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during quiz generation: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("history")]
    public async Task<IActionResult> SaveQuizHistory([FromBody] SaveQuizHistoryRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var historyRecord = new QuizHistoryRecord
        {
            UserId = userId,
            FileName = request.FileName,
            Difficulty = request.Difficulty,
            TotalQuestions = request.Questions.Count,
            Score = request.Score,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var qDto in request.Questions)
        {
            var question = new QuizHistoryQuestion
            {
                QuestionText = qDto.QuestionText,
                CorrectAnswerIndex = qDto.CorrectAnswerIndex,
                UserAnswerIndex = qDto.UserAnswerIndex
            };

            for (int i = 0; i < qDto.Options.Count; i++)
            {
                question.Options.Add(new QuizHistoryOption
                {
                    OptionText = qDto.Options[i],
                    Index = i
                });
            }

            historyRecord.Questions.Add(question);
        }

        _context.QuizHistories.Add(historyRecord);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Quiz history saved successfully.", HistoryId = historyRecord.Id });
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetQuizHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var history = await _context.QuizHistories
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuizHistoryListDto
            {
                Id = q.Id,
                FileName = q.FileName,
                Difficulty = q.Difficulty,
                TotalQuestions = q.TotalQuestions,
                Score = q.Score,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();

        return Ok(history);
    }

    [Authorize]
    [HttpGet("history/{id}")]
    public async Task<IActionResult> GetQuizHistoryDetails(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var record = await _context.QuizHistories
            .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (record == null)
        {
            return NotFound("Quiz history not found.");
        }

        var details = new QuizHistoryDetailsDto
        {
            Id = record.Id,
            FileName = record.FileName,
            Difficulty = record.Difficulty,
            Score = record.Score,
            CreatedAt = record.CreatedAt,
            Questions = record.Questions.Select(q => new QuizHistoryQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                CorrectAnswerIndex = q.CorrectAnswerIndex,
                UserAnswerIndex = q.UserAnswerIndex,
                Options = q.Options.OrderBy(o => o.Index).Select(o => o.OptionText).ToList()
            }).ToList()
        };

        return Ok(details);
    }
}
