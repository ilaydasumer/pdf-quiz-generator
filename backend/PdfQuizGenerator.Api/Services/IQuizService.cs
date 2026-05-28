using System.Collections.Generic;
using System.Threading.Tasks;
using PdfQuizGenerator.Api.Models;

namespace PdfQuizGenerator.Api.Services;

public interface IQuizService
{
    Task<List<QuizQuestion>> GenerateQuizAsync(string fileName, int questionCount);
}
