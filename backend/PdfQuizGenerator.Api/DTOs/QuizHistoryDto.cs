using System;
using System.Collections.Generic;

namespace PdfQuizGenerator.Api.DTOs;

public class SaveQuizHistoryRequest
{
    public string FileName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<SaveQuizQuestionDto> Questions { get; set; } = new();
}

public class SaveQuizQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public int CorrectAnswerIndex { get; set; }
    public int UserAnswerIndex { get; set; }
    public List<string> Options { get; set; } = new();
}

public class QuizHistoryListDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QuizHistoryDetailsDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuizHistoryQuestionDto> Questions { get; set; } = new();
}

public class QuizHistoryQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int CorrectAnswerIndex { get; set; }
    public int UserAnswerIndex { get; set; }
    public List<string> Options { get; set; } = new();
}
