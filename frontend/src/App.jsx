import React, { useState } from 'react';
import UploadBox from './components/UploadBox';
import QuizSettings from './components/QuizSettings';
import QuizQuestion from './components/QuizQuestion';
import ResultScreen from './components/ResultScreen';
import { mockQuestions } from './data/mockQuestions';

const BACKEND_URL = window.location.hostname === '127.0.0.1' 
  ? 'http://127.0.0.1:5292/api/quiz/generate' 
  : 'http://localhost:5292/api/quiz/generate';

export default function App() {
  const [step, setStep] = useState('setup'); // 'setup' | 'loading' | 'quiz' | 'result'
  const [file, setFile] = useState(null);
  const [questionCount, setQuestionCount] = useState(5);
  const [difficulty, setDifficulty] = useState('Medium');
  const [offlineMode, setOfflineMode] = useState(false);
  const [questions, setQuestions] = useState([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState({});
  const [error, setError] = useState('');
  const [isGenerating, setIsGenerating] = useState(false);

  const handleGenerate = async () => {
    if (!file && !offlineMode) {
      return;
    }

    setError('');
    setIsGenerating(true);
    setStep('loading');

    try {
      if (offlineMode) {
        // Simulate network delay for offline mock generation
        await new Promise((resolve) => setTimeout(resolve, 1200));
        // Take the requested number of questions from mock pool
        const slicedMock = mockQuestions.slice(0, questionCount);
        setQuestions(slicedMock);
        setUserAnswers({});
        setCurrentQuestionIndex(0);
        setStep('quiz');
      } else {
        // Call the real C# Backend API
        const formData = new FormData();
        formData.append('File', file);
        formData.append('QuestionCount', questionCount.toString());
        formData.append('Difficulty', difficulty);

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 60000); // 60s timeout

        const response = await fetch(BACKEND_URL, {
          method: 'POST',
          body: formData,
          signal: controller.signal
        });

        clearTimeout(timeoutId);

        if (!response.ok) {
          const errMsg = await response.text();
          throw new Error(errMsg || `API returned status code ${response.status}`);
        }

        const data = await response.json();
        setQuestions(data);
        setUserAnswers({});
        setCurrentQuestionIndex(0);
        setStep('quiz');
      }
    } catch (err) {
      console.error(err);
      if (err.name === 'AbortError') {
        setError('Request timed out after 60 seconds. The AI might be taking too long or the backend is unreachable.');
      } else if (err.message === 'Failed to fetch') {
        setError('Backend server is not reachable. Please start the ASP.NET Core API or enable Offline Mode.');
      } else {
        setError(`Backend Error: ${err.message}`);
      }
      setStep('setup');
    } finally {
      setIsGenerating(false);
    }
  };

  const handleAnswerSelect = (optionIndex) => {
    setUserAnswers((prev) => ({
      ...prev,
      [currentQuestionIndex]: optionIndex,
    }));
  };

  const handlePrev = () => {
    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex((prev) => prev - 1);
    }
  };

  const handleNext = () => {
    if (currentQuestionIndex < questions.length - 1) {
      setCurrentQuestionIndex((prev) => prev + 1);
    }
  };

  const handleSubmit = () => {
    // Validate that all questions are answered
    if (Object.keys(userAnswers).length < questions.length) {
      alert('Please answer all questions before submitting.');
      return;
    }
    setStep('result');
  };

  const handleRestart = (keepFile = false) => {
    setQuestions([]);
    setUserAnswers({});
    setCurrentQuestionIndex(0);
    setError('');
    if (!keepFile) setFile(null);
    setStep('setup');
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <span className="app-logo">📝</span>
        <h1 className="app-title">PDF Quiz Generator</h1>
        <p className="app-description">
          Upload a study guide, textbook chapter, or article in PDF format, choose your options, and generate a customized quiz instantly.
        </p>
      </header>

      <main className="app-content">
        {step === 'setup' && (
          <div className="setup-workflow">
            <UploadBox file={file} setFile={setFile} />
            
            <QuizSettings
              questionCount={questionCount}
              setQuestionCount={setQuestionCount}
              difficulty={difficulty}
              setDifficulty={setDifficulty}
              offlineMode={offlineMode}
              setOfflineMode={setOfflineMode}
              onGenerate={handleGenerate}
              disabled={!file && !offlineMode}
              isGenerating={isGenerating}
            />

            {error && (
              <div className="card error-card">
                <h4 className="error-title">Connection Error</h4>
                <p className="error-text">{error}</p>
              </div>
            )}
          </div>
        )}

        {step === 'loading' && (
          <div className="card loading-card">
            <div className="spinner"></div>
            <p className="loading-text">Analyzing PDF & Generating Quiz</p>
            <p className="loading-subtext">Creating {questionCount} custom questions based on your file...</p>
          </div>
        )}

        {step === 'quiz' && (
          <QuizQuestion
            question={questions[currentQuestionIndex]}
            questionIndex={currentQuestionIndex}
            totalQuestions={questions.length}
            selectedAnswer={userAnswers[currentQuestionIndex]}
            onSelectAnswer={handleAnswerSelect}
            onPrev={handlePrev}
            onNext={handleNext}
            onSubmit={handleSubmit}
            isLast={currentQuestionIndex === questions.length - 1}
          />
        )}

        {step === 'result' && (
          <ResultScreen
            questions={questions}
            userAnswers={userAnswers}
            onRestart={() => handleRestart(true)}
            onNewUpload={() => handleRestart(false)}
          />
        )}
      </main>

      <footer className="app-footer">
        <p>© {new Date().getFullYear()} PDF Quiz Generator. A modern learning prototype.</p>
      </footer>
    </div>
  );
}
