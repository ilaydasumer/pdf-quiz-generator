using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PdfQuizGenerator.Api.Models.Entities;

namespace PdfQuizGenerator.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<QuizHistoryRecord> QuizHistories { get; set; }
    public DbSet<QuizHistoryQuestion> QuizHistoryQuestions { get; set; }
    public DbSet<QuizHistoryOption> QuizHistoryOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<QuizHistoryRecord>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizHistoryQuestion>()
            .HasOne(q => q.QuizHistoryRecord)
            .WithMany(r => r.Questions)
            .HasForeignKey(q => q.QuizHistoryRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizHistoryOption>()
            .HasOne(o => o.QuizHistoryQuestion)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuizHistoryQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
