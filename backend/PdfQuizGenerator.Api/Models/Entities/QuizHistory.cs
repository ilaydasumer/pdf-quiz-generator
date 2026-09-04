using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PdfQuizGenerator.Api.Models.Entities;

public class QuizHistoryRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<QuizHistoryQuestion> Questions { get; set; } = new();
    
    [JsonIgnore]
    public ApplicationUser? User { get; set; }
}

public class QuizHistoryQuestion
{
    public int Id { get; set; }
    public int QuizHistoryRecordId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int CorrectAnswerIndex { get; set; }
    public int UserAnswerIndex { get; set; } // Store what the user answered

    public List<QuizHistoryOption> Options { get; set; } = new();

    [JsonIgnore]
    public QuizHistoryRecord? QuizHistoryRecord { get; set; }
}

public class QuizHistoryOption
{
    public int Id { get; set; }
    public int QuizHistoryQuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int Index { get; set; }

    [JsonIgnore]
    public QuizHistoryQuestion? QuizHistoryQuestion { get; set; }
}
