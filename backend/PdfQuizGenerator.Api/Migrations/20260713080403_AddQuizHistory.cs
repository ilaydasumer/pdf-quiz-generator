using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PdfQuizGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizHistoryQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizHistoryRecordId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    CorrectAnswerIndex = table.Column<int>(type: "integer", nullable: false),
                    UserAnswerIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizHistoryQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizHistoryQuestions_QuizHistories_QuizHistoryRecordId",
                        column: x => x.QuizHistoryRecordId,
                        principalTable: "QuizHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizHistoryOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizHistoryQuestionId = table.Column<int>(type: "integer", nullable: false),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizHistoryOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizHistoryOptions_QuizHistoryQuestions_QuizHistoryQuestion~",
                        column: x => x.QuizHistoryQuestionId,
                        principalTable: "QuizHistoryQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizHistories_UserId",
                table: "QuizHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizHistoryOptions_QuizHistoryQuestionId",
                table: "QuizHistoryOptions",
                column: "QuizHistoryQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizHistoryQuestions_QuizHistoryRecordId",
                table: "QuizHistoryQuestions",
                column: "QuizHistoryRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizHistoryOptions");

            migrationBuilder.DropTable(
                name: "QuizHistoryQuestions");

            migrationBuilder.DropTable(
                name: "QuizHistories");
        }
    }
}
