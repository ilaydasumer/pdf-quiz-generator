using System.IO;

namespace PdfQuizGenerator.Api.Services;

public interface IPdfTextExtractor
{
    string ExtractText(Stream pdfStream);
}
