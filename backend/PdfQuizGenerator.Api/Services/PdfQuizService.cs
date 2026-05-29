using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PdfQuizGenerator.Api.Models;

namespace PdfQuizGenerator.Api.Services;

public class PdfQuizService : IQuizService
{
    private readonly IPdfTextExtractor _pdfTextExtractor;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't",
        "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
        "can", "can't", "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing",
        "don't", "down", "during", "each", "few", "for", "from", "further", "had", "hadn't", "has", "hasn't",
        "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself",
        "him", "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is",
        "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most", "mustn't", "my", "myself",
        "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves",
        "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so",
        "some", "such", "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there",
        "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to",
        "too", "under", "until", "up", "very", "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were",
        "weren't", "what", "what's", "when", "when's", "where", "where's", "which", "while", "who", "who's",
        "whom", "why", "why's", "with", "won't", "would", "wouldn't", "you", "you'd", "you'll", "you're",
        "you've", "your", "yours", "yourself", "yourselves", "using", "project", "value", "public",
        "return", "method", "class", "namespace", "string", "program"
    };

    private static readonly HashSet<string> GenericTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "content", "elements", "text", "system", "data", "process", "question", "example", "chapter", 
        "section", "figure", "table", "information", "details", "document", "this document", 
        "the content", "the text", "the section", "a system", "this chapter", "this section", "the system"
    };

    private static readonly HashSet<string> MeaningfulCsTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "Binary Search", "Hash Table", "Stack", "Queue", "Graph", "Graphs", "Primary Key", "Foreign Key", 
        "SQL", "Deadlock", "DNS", "TCP", "HTTP", "Machine Learning", "RAG", "LLM", "Algorithm", "Database", 
        "Operating System", "Compiler", "Network", "Time Complexity", "Big O Notation", "Time complexity", "Big O notation"
    };

    private static readonly HashSet<string> KnownAcronyms = new(StringComparer.OrdinalIgnoreCase)
    {
        "SQL", "DNS", "TCP", "UDP", "HTTP", "AI", "OS", "IP", "API", "CPU", "RAM", "ROM", "SSD", "HDD", "URL", "URI", "XML", "JSON", "OOP", "RAG", "LLM", "ML", "UI", "UX"
    };

    private static readonly List<string> FallbackDistractors = new()
    {
        "Framework", "Database", "Algorithm", "Variable", "Function", "Compiler", "Server", "Protocol",
        "Application", "Interface", "Object", "Class", "Method", "Inheritance", "Polymorphism"
    };

    private static readonly List<string> FallbackDefinitions = new()
    {
        "a structured set of data held in a computer, especially one that is accessible in various ways",
        "a process or set of rules to be followed in calculations or other problem-solving operations",
        "a program that translates code written in one programming language into another language",
        "a system of rules that allow two or more entities of a communications system to transmit information",
        "a style or way of programming that is associated with a particular set of concepts"
    };

    public PdfQuizService(IPdfTextExtractor pdfTextExtractor)
    {
        _pdfTextExtractor = pdfTextExtractor ?? throw new ArgumentNullException(nameof(pdfTextExtractor));
    }

    public async Task<List<QuizQuestion>> GenerateQuizAsync(Stream pdfStream, int questionCount)
    {
        var extractedText = _pdfTextExtractor.ExtractText(pdfStream);
        await Task.Delay(1000);

        int targetCount = Math.Clamp(questionCount, 5, 15);

        var allSentences = SplitIntoSentences(extractedText);
        var definitionCandidates = new List<(string Term, string Definition, string QuestionPrefix)>();
        var allWordsInText = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sentence in allSentences)
        {
            if (TryParseDefinition(sentence, out var term, out var def, out var prefix))
            {
                definitionCandidates.Add((term, def, prefix));
            }

            var words = sentence.Split(new[] { ' ', ',', '.', ';', ':', '(', ')', '"', '\'', '-', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in words)
            {
                var clean = w.Trim().TrimEnd('.', ',', '!', '?');
                if (clean.Length >= 6 && !IsStopWord(clean) && IsPurelyAlphabetic(clean))
                {
                    allWordsInText.Add(clean);
                }
            }
        }

        var generatedQuestions = new List<QuizQuestion>();
        var rand = new Random();
        int idCounter = 1;
        var usedSentences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var defCandidate in definitionCandidates)
        {
            if (generatedQuestions.Count >= targetCount) break;

            var term = defCandidate.Term;
            var definition = defCandidate.Definition;
            var prefix = defCandidate.QuestionPrefix;

            var otherDefs = definitionCandidates
                .Where(d => !d.Term.Equals(term, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.Definition)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            while (otherDefs.Count < 3)
            {
                var fallback = FallbackDefinitions[rand.Next(FallbackDefinitions.Count)];
                if (!otherDefs.Contains(fallback))
                {
                    otherDefs.Add(fallback);
                }
            }

            var distractors = otherDefs.OrderBy(_ => rand.Next()).Take(3).ToList();
            var options = new List<string> { definition };
            options.AddRange(distractors);

            options = options.OrderBy(_ => rand.Next()).ToList();
            int correctIndex = options.IndexOf(definition);

            options = options.Select(o => o.Length > 0 ? char.ToUpper(o[0]) + o.Substring(1) : o).ToList();

            generatedQuestions.Add(new QuizQuestion
            {
                Id = idCounter++,
                QuestionText = prefix,
                Options = options,
                CorrectAnswerIndex = correctIndex
            });
        }

        foreach (var sentence in allSentences)
        {
            if (generatedQuestions.Count >= targetCount) break;
            if (usedSentences.Contains(sentence)) continue;

            if (TryCreateFillInTheBlank(sentence, allWordsInText.ToList(), out var questionText, out var correctAnswer))
            {
                if (generatedQuestions.Any(q => q.Options[q.CorrectAnswerIndex].Equals(correctAnswer, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var otherWords = allWordsInText
                    .Where(w => !w.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                while (otherWords.Count < 3)
                {
                    var fallback = FallbackDistractors[rand.Next(FallbackDistractors.Count)];
                    if (!otherWords.Contains(fallback))
                    {
                        otherWords.Add(fallback);
                    }
                }

                var distractors = otherWords.OrderBy(_ => rand.Next()).Take(3).ToList();
                var options = new List<string> { correctAnswer };
                options.AddRange(distractors);

                options = options.OrderBy(_ => rand.Next()).ToList();
                int correctIndex = options.IndexOf(correctAnswer);

                generatedQuestions.Add(new QuizQuestion
                {
                    Id = idCounter++,
                    QuestionText = questionText,
                    Options = options,
                    CorrectAnswerIndex = correctIndex
                });

                usedSentences.Add(sentence);
            }
        }

        while (generatedQuestions.Count < targetCount)
        {
            var generalQ = GetNextFallbackQuestion(idCounter++, generatedQuestions);
            generatedQuestions.Add(generalQ);
        }

        return generatedQuestions;
    }

    private List<string> SplitIntoSentences(string text)
    {
        var normalized = Regex.Replace(text, @"\s+", " ");
        var matches = Regex.Split(normalized, @"(?<=[.!?])\s+(?=[A-Z])");

        var result = new List<string>();
        foreach (var m in matches)
        {
            var clean = m.Trim();
            if (clean.Length >= 30 && clean.Length <= 200)
            {
                result.Add(clean);
            }
        }
        return result;
    }

    private bool TryParseDefinition(string sentence, out string term, out string definition, out string questionPrefix)
    {
        term = string.Empty;
        definition = string.Empty;
        questionPrefix = string.Empty;

        var copulas = new[] { " is a ", " is an ", " is the ", " is ", " are ", " refers to ", " is defined as ", " describes " };
        foreach (var copula in copulas)
        {
            var idx = sentence.IndexOf(copula, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                var left = sentence.Substring(0, idx).Trim();
                var right = sentence.Substring(idx + copula.Length).Trim();

                left = left.TrimStart('"', '\'', ' ', '-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.');
                right = right.TrimEnd('"', '\'', ' ', '.', ',', ';');

                // 2. Clean duplicated title-term patterns
                var colonIdx = left.IndexOf(':');
                if (colonIdx > 0)
                {
                    var parts = left.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var p0 = parts[0].Trim();
                        var p1 = parts.Length > 1 ? parts[1].Trim() : "";
                        
                        if (!string.IsNullOrEmpty(p1))
                        {
                            var cleanedP1 = p1;
                            if (cleanedP1.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) cleanedP1 = cleanedP1.Substring(2).Trim();
                            if (cleanedP1.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) cleanedP1 = cleanedP1.Substring(4).Trim();
                            
                            if (string.Equals(p0, cleanedP1, StringComparison.OrdinalIgnoreCase) || 
                                p1.StartsWith("A ", StringComparison.OrdinalIgnoreCase) || 
                                p1.StartsWith("The ", StringComparison.OrdinalIgnoreCase))
                            {
                                left = p0;
                            }
                            else
                            {
                                left = p1 != "" ? p1 : p0;
                            }
                        }
                        else
                        {
                            left = p0;
                        }
                    }
                }

                // 4. Avoid using terms starting with "The" (by stripping articles)
                if (left.StartsWith("The ", StringComparison.OrdinalIgnoreCase)) left = left.Substring(4).Trim();
                else if (left.StartsWith("A ", StringComparison.OrdinalIgnoreCase)) left = left.Substring(2).Trim();
                else if (left.StartsWith("An ", StringComparison.OrdinalIgnoreCase)) left = left.Substring(3).Trim();
                else if (left.StartsWith("This ", StringComparison.OrdinalIgnoreCase)) left = left.Substring(5).Trim();

                // 1. Never create questions from generic phrases
                if (GenericTerms.Contains(left) && !MeaningfulCsTerms.Contains(left)) continue;
                
                if (left.Length < 3 && !KnownAcronyms.Contains(left) && !MeaningfulCsTerms.Contains(left)) continue;

                var dotIdx = right.IndexOf('.');
                if (dotIdx > 0) right = right.Substring(0, dotIdx);
                
                var leftWordCount = left.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                
                if (leftWordCount >= 1 && leftWordCount <= 4 && right.Length > 15 && left.Length >= 2)
                {
                    if (char.IsLetterOrDigit(left[0]))
                    {
                        term = left;
                        definition = right;
                        
                        var termLower = term.ToLowerInvariant();
                        bool describes = termLower.Contains("notation") || termLower.Contains("complexity");
                        bool noArticle = termLower.Contains("search") || termLower.Contains("sort") || termLower.Contains("learning") || termLower.Contains("ai");
                        
                        // 3. Generate cleaner question prompts
                        if (copula.Equals(" refers to ", StringComparison.OrdinalIgnoreCase) || copula.Equals(" is defined as ", StringComparison.OrdinalIgnoreCase))
                            questionPrefix = $"What does '{term}' refer to?";
                        else if (copula.Equals(" describes ", StringComparison.OrdinalIgnoreCase) || describes)
                            questionPrefix = $"What does {termLower} describe?";
                        else
                        {
                            var vowels = new[] { 'A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u' };
                            bool isPlural = termLower.EndsWith("s") && !termLower.EndsWith("ss");
                            
                            if (noArticle)
                                questionPrefix = $"What is {termLower}?";
                            else if (isPlural)
                                questionPrefix = $"What are {termLower}?";
                            else if (vowels.Contains(term[0]))
                                questionPrefix = $"What is an {termLower}?";
                            else
                                questionPrefix = $"What is a {termLower}?";
                        }
                            
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool TryCreateFillInTheBlank(string sentence, List<string> allCandidateWords, out string questionText, out string correctAnswer)
    {
        questionText = string.Empty;
        correctAnswer = string.Empty;

        var words = sentence.Split(new[] { ' ', ',', '.', ';', ':', '(', ')', '"', '\'', '-', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var candidates = words
            .Select(w => w.Trim())
            .Where(w => w.Length >= 6 && !IsStopWord(w) && IsPurelyAlphabetic(w) && !GenericTerms.Contains(w))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidates.Count == 0) return false;

        var chosenWord = candidates.OrderByDescending(c => c.Length).First();

        var regexPattern = @"\b" + Regex.Escape(chosenWord) + @"\b";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        if (!regex.IsMatch(sentence)) return false;

        var blankedSentence = regex.Replace(sentence, "_____", 1);
        questionText = $"Which concept best completes the sentence?\n\"{blankedSentence}\"";
        correctAnswer = chosenWord;
        return true;
    }

    private bool IsStopWord(string word)
    {
        return StopWords.Contains(word);
    }

    private bool IsPurelyAlphabetic(string word)
    {
        return word.All(char.IsLetter);
    }

    private QuizQuestion GetNextFallbackQuestion(int id, List<QuizQuestion> existing)
    {
        var pool = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                QuestionText = "What is the primary purpose of version control systems like Git?",
                Options = new() { "To track changes in source code", "To host web applications", "To compile code", "To design user interfaces" },
                CorrectAnswerIndex = 0
            },
            new QuizQuestion
            {
                QuestionText = "What does Big O notation describe?",
                Options = new() { "Algorithm performance and scalability", "Database storage formats", "Network bandwidth usage", "Memory addresses" },
                CorrectAnswerIndex = 0
            },
            new QuizQuestion
            {
                QuestionText = "Which concept is the smallest unit of execution within a process?",
                Options = new() { "Thread", "Program", "Function", "Variable" },
                CorrectAnswerIndex = 0
            },
            new QuizQuestion
            {
                QuestionText = "In programming, what is a hash table used for?",
                Options = new() { "Fast data retrieval using key-value pairs", "Sorting numerical arrays", "Creating user interfaces", "Establishing database connections" },
                CorrectAnswerIndex = 0
            },
            new QuizQuestion
            {
                QuestionText = "What does database indexing primarily improve?",
                Options = new() { "Query execution speed", "Data storage capacity", "Database security", "Network bandwidth" },
                CorrectAnswerIndex = 0
            }
        };

        foreach (var q in pool)
        {
            if (!existing.Any(e => e.QuestionText.Equals(q.QuestionText, StringComparison.OrdinalIgnoreCase)))
            {
                q.Id = id;
                return q;
            }
        }

        var defaultQ = pool[0];
        defaultQ.Id = id;
        return defaultQ;
    }
}
