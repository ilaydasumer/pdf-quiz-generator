using System.Threading.Tasks;
using PdfQuizGenerator.Api.DTOs;

namespace PdfQuizGenerator.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
