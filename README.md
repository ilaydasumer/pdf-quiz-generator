# PDF Quiz Generator

A modern, full-stack application designed to generate multiple-choice quiz questions from uploaded PDF files, allowing users to select question counts, take the quiz, submit answers, and receive detailed scoring feedback.

This repository contains a working prototype designed as a portfolio-ready demonstration, complete with rule-based text extraction logic on the backend and an intuitive SaaS-inspired web interface.

---

## 🛠️ Technologies Used

### Frontend
- **React (v18)** - Dynamic, component-driven user interface.
- **Vite** - High-speed frontend building and hot module replacement.
- **Vanilla CSS** - Customized SaaS-style layout, modern typography, responsive cards, and clean hover state transitions.

### Backend
- **ASP.NET Core Web API (.NET 9)** - Secure, structured RESTful API.
- **C#** - Strongly-typed server-side controller and service pipeline.
- **UglyToad.PdfPig** - Powerful PDF parsing library for text extraction.
- **OpenAPI (Swagger)** - In-browser API documentation.

---

## 🚀 Features

### Current Version (v1.0 - Working Prototype)
- **Real PDF Text Extraction**: Uses `PdfPig` to extract text blocks, sentences, and keywords, processing them with a rule-based algorithm.
- *(Note: Scanned or image-only PDFs currently require OCR and are not fully supported yet.)*
- **SaaS-Style UI**: A clean, distraction-free centered card layout with responsive CSS adjustments for mobile devices.
- **PDF Upload Zone**: Interactive drag-and-drop or file browser module validating PDF formats.
- **Count Selector**: Options to choose between 5, 10, or 15 questions.
- **Interactive Quiz Player**: Options card select states, previous/next question navigation, and completion progress bars.
- **Comprehensive Score review**: Percentage results, custom feedback messages, and a list highlighting correctly and incorrectly answered questions.
- **Dual-Mode System**:
  - **API Mode**: Queries the ASP.NET Core server dynamically over CORS to extract text and generate questions.
  - **Offline Mode**: A client-side mock questions fallback allowing instant testing of the frontend without running the server.
- **CORS Configured**: Configured to connect seamlessly across local dev ports.

### Planned Features (Future Roadmap)
- [ ] **AI-Powered Quiz Generation**: Integrate Gemini or OpenAI APIs to analyze extracted text and generate high-quality conceptual questions dynamically, moving beyond rule-based extraction.
- [ ] **OCR Support**: Add optical character recognition to support scanned documents.
- [ ] **User Authentication**: Implement JWT/Identity authentication for users.
- [ ] **Quiz History**: Save user quizzes and results to a database (SQL Server/PostgreSQL).
- [ ] **Leaderboard**: Track high scores across users on public quiz guides.

---

## 💻 Running the Project

### 1. Starting the C# Backend Web API

Navigate to the API folder and run the command:

```bash
cd backend/PdfQuizGenerator.Api
dotnet run
```

The server will start and run locally on:
- HTTP: `http://localhost:5292`
- HTTPS: `https://localhost:7217`

You can verify the API is up by visiting the Swagger OpenAPI dashboard at:
`http://localhost:5292/openapi/v1.json`

---

### 2. Starting the React Frontend

Navigate to the frontend folder, install the dependencies, and start the Vite dev server:

```bash
cd frontend
npm install
npm run dev
```

The web client will open on:
`http://localhost:5173`

*(Note: If the backend is running, the frontend will connect automatically. If you run the frontend alone, select **"Use Local Mock Questions (Offline Mode)"** in the settings checklist to test).*

---

## 🔗 Example API Endpoint

### Generate Quiz
- **Path**: `POST /api/quiz/generate`
- **Content-Type**: `multipart/form-data`
- **Request Parameters**:
  - `File`: PDF File Attachment (`application/pdf`)
  - `QuestionCount`: Integer (`5`, `10`, or `15`)

#### Sample Response DTO (`200 OK`)
```json
[
  {
    "id": 1,
    "questionText": "What does HTML stand for?",
    "options": [
      { "index": 0, "text": "HyperText Markup Language" },
      { "index": 1, "text": "HighText Markup Language" },
      { "index": 2, "text": "HyperText Markdown Language" },
      { "index": 3, "text": "HyperText Multiple Language" }
    ],
    "correctAnswerIndex": 0
  },
  {
    "id": 2,
    "questionText": "Which language is primarily used for styling web pages?",
    "options": [
      { "index": 0, "text": "JavaScript" },
      { "index": 1, "text": "HTML" },
      { "index": 2, "text": "CSS" },
      { "index": 3, "text": "Python" }
    ],
    "correctAnswerIndex": 2
  }
]
```
