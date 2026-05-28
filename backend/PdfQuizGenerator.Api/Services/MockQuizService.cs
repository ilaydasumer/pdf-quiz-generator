using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PdfQuizGenerator.Api.Models;

namespace PdfQuizGenerator.Api.Services;

public class MockQuizService : IQuizService
{
    private static readonly List<QuizQuestion> MockQuestionsPool = new()
    {
        new QuizQuestion
        {
            Id = 1,
            QuestionText = "What does HTML stand for?",
            Options = new() { "HyperText Markup Language", "HighText Markup Language", "HyperText Markdown Language", "HyperText Multiple Language" },
            CorrectAnswerIndex = 0
        },
        new QuizQuestion
        {
            Id = 2,
            QuestionText = "Which language is primarily used for styling web pages?",
            Options = new() { "JavaScript", "HTML", "CSS", "Python" },
            CorrectAnswerIndex = 2
        },
        new QuizQuestion
        {
            Id = 3,
            QuestionText = "What does API stand for?",
            Options = new() { "Application Programming Interface", "Applied Protocol Integration", "Application Process Interconnect", "Access Program Interface" },
            CorrectAnswerIndex = 0
        },
        new QuizQuestion
        {
            Id = 4,
            QuestionText = "Which keyword in C# is used to declare a read-only variable whose value is evaluated at compile-time?",
            Options = new() { "readonly", "const", "static", "volatile" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 5,
            QuestionText = "What does CORS stand for in web development?",
            Options = new() { "Cross-Origin Resource Sharing", "Core Object Routing System", "Client Origin Resource Security", "Common Origin Request Server" },
            CorrectAnswerIndex = 0
        },
        new QuizQuestion
        {
            Id = 6,
            QuestionText = "Which of the following is NOT a JavaScript framework or library?",
            Options = new() { "React", "Vue", "Angular", "Django" },
            CorrectAnswerIndex = 3
        },
        new QuizQuestion
        {
            Id = 7,
            QuestionText = "In CSS layout, what is the default value of the flex-direction property?",
            Options = new() { "column", "row", "row-reverse", "column-reverse" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 8,
            QuestionText = "What is the primary role of the Program.cs file in a modern .NET Core Web API?",
            Options = new() { "To hold database connections", "To configure services and define the application pipeline", "To store CSS styles", "To compile frontend bundles" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 9,
            QuestionText = "Which HTTP method is designed to be idempotent and is primarily used to retrieve data?",
            Options = new() { "POST", "GET", "PUT", "DELETE" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 10,
            QuestionText = "Which linear data structure follows the Last-In, First-Out (LIFO) access pattern?",
            Options = new() { "Queue", "Stack", "Linked List", "Binary Tree" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 11,
            QuestionText = "What does DOM stand for in frontend development?",
            Options = new() { "Data Object Mapping", "Document Object Model", "Dynamic Operations Module", "Distributed Object Management" },
            CorrectAnswerIndex = 1
        },
        new QuizQuestion
        {
            Id = 12,
            QuestionText = "Which SQL clause is used to filter the groups returned by a GROUP BY clause?",
            Options = new() { "WHERE", "ORDER BY", "HAVING", "LIMIT" },
            CorrectAnswerIndex = 2
        },
        new QuizQuestion
        {
            Id = 13,
            QuestionText = "What is the primary function of Git?",
            Options = new() { "Hosting web servers", "Running automated database backups", "Tracking changes in source code (Version Control)", "Compiling typescript files" },
            CorrectAnswerIndex = 2
        },
        new QuizQuestion
        {
            Id = 14,
            QuestionText = "Which of the following is a widely-used relational database management system?",
            Options = new() { "PostgreSQL", "MongoDB", "Redis", "Cassandra" },
            CorrectAnswerIndex = 0
        },
        new QuizQuestion
        {
            Id = 15,
            QuestionText = "In C#, what is the base class for all exceptions?",
            Options = new() { "System.Error", "System.Exception", "System.BaseException", "System.Throwable" },
            CorrectAnswerIndex = 1
        }
    };

    public async Task<List<QuizQuestion>> GenerateQuizAsync(string fileName, int questionCount)
    {
        // Simulate extraction and AI generation delay (e.g. 1.5 seconds)
        await Task.Delay(1500);

        // Safely determine how many questions to return
        int count = Math.Clamp(questionCount, 5, 15);

        // Returns the first N questions from the pool.
        // In a real application, you'd extract PDF text and pass it to an AI model.
        return MockQuestionsPool.Take(count).ToList();
    }
}
