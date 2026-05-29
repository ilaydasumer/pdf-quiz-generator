using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PdfQuizGenerator.Api.Models;

namespace PdfQuizGenerator.Api.Services;

public interface IQuizService
{
    Task<List<QuizQuestion>> GenerateQuizAsync(Stream pdfStream, int questionCount);
}
