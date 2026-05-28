using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PdfQuizGenerator.Api.DTOs;

public class QuizGenerateRequest
{
    [Required(ErrorMessage = "PDF file is required.")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "Question count is required.")]
    [Range(5, 15, ErrorMessage = "Question count must be 5, 10, or 15.")]
    public int QuestionCount { get; set; }
}
