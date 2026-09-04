# PDF Quiz Generator 📝

PDF Quiz Generator is a full-stack application that generates multiple-choice quizzes from PDF documents.

The user uploads a text-based PDF, the backend extracts the document content using `PdfPig`, and the extracted text is sent to the Google Gemini API to generate questions based on the document.

The goal of the project is to provide a simple way for students, teachers, or anyone studying from long documents to quickly test their understanding.

## What It Does 🚀

* **Quiz Generation:** Upload a text-based PDF and generate multiple-choice questions based on its content.
* **Custom Question Count:** Choose exactly how many questions you want to solve (5, 10, or 15).
* **User Authentication:** Users can register and log in securely using ASP.NET Core Identity and JWT authentication.
* **Quiz History:** Generated quizzes and user scores are stored in PostgreSQL so previous results can be reviewed later.
* **Simple UI:** The frontend is built with React and vanilla CSS without using an additional bulky UI framework.

## How It Works ⚙️

1. The user uploads a PDF from the React frontend.
2. The file is sent to the ASP.NET Core backend.
3. `PdfPig` extracts the text layer from the PDF.
4. The extracted content is sent to the Gemini 2.5 API with a strict system prompt.
5. Gemini generates multiple-choice questions in a specific JSON format based on the document.
6. The backend parses the JSON and returns the quiz to the frontend.
7. After the quiz is completed, the result is stored in PostgreSQL, linked to the authenticated user.

## Under the Hood (Technical Details) 🧠

To make the application robust and reliable, a few key technical decisions were implemented:

* **Text-Only Payload:** The backend does not send the raw PDF file to the AI. Instead, it only sends the extracted text. To prevent exceeding Gemini's token limits, the text payload is strictly clamped to a maximum of 100,000 characters.
* **Strict JSON Response:** The AI is prompted to return a strictly typed JSON array without markdown formatting. Each generated question adheres to the following structure:
  ```json
  {
    "questionText": "What is the capital of France?",
    "options": ["London", "Berlin", "Paris", "Madrid"],
    "correctAnswerIndex": 2
  }
  ```
* **Database Relationships:** A one-to-many relationship is established in Entity Framework Core between the `ApplicationUser` (Identity) and `QuizHistory` entities, allowing users to securely retrieve only their own past quiz scores.

## Tech Stack 🛠️

### Backend
* ASP.NET Core Web API (.NET 9)
* Entity Framework Core & PostgreSQL
* ASP.NET Core Identity & JWT Authentication
* Google Gemini API
* PdfPig

### Frontend
* React 18
* Vite
* Axios
* Vanilla CSS

## How to Run It Locally 💻

### Requirements
Make sure you have the following installed:
* .NET 9 SDK
* Node.js
* PostgreSQL

### 1. Backend Setup

Navigate to the API project:
```bash
cd backend/PdfQuizGenerator.Api
```

Create a `.env` file inside the API folder and add the required configuration:
```env
GEMINI_API_KEY=your_gemini_api_key_here
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=PdfQuizGeneratorDb;Username=postgres;Password=yourpassword
JwtSettings__Secret=YourSuperSecretJWTKeyThatIsVeryLong
```

Apply the Entity Framework migrations:
```bash
dotnet ef database update
```

Start the backend:
```bash
dotnet run
```
The API runs by default at `http://localhost:5292`. 
The OpenAPI specification is available at `http://localhost:5292/openapi/v1.json`.

### 2. Frontend Setup

Open a new terminal and navigate to the frontend folder:
```bash
cd frontend
```

Install the dependencies and start the development server:
```bash
npm install
npm run dev
```
The frontend runs by default at `http://localhost:5173`.

## Current Limitations ⚠️

The application currently supports text-based PDF files only.

Scanned PDFs or PDFs that mainly contain images are not supported yet because OCR (Optical Character Recognition) functionality has not been implemented. `PdfPig` relies on embedded fonts to extract text.

## Future Plans 🗺️
* [ ] Add OCR support for scanned or image-based PDFs
* [ ] Add a public leaderboard
* [ ] Export generated quizzes as PDF
* [ ] Export quiz results as CSV
* [ ] Add more quiz customization options (difficulty levels, question types)

## Contributing 🤝
Bug reports, suggestions, and contributions are welcome. Feel free to open an issue or submit a pull request!
