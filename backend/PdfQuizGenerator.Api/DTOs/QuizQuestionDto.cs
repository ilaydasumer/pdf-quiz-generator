using System.Collections.Generic;

namespace PdfQuizGenerator.Api.DTOs;

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<QuizOptionDto> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
}
