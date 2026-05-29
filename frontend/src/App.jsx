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
  const [offlineMode, setOfflineMode] = useState(false);
  const [questions, setQuestions] = useState([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState({});
  const [error, setError] = useState('');

  const handleGenerate = async () => {
    if (!file) {
      setError('Please upload a PDF file first.');
      return;
    }

    setError('');
    setStep('loading');

    if (offlineMode) {
      // Simulate network delay for offline mock generation
      setTimeout(() => {
        // Take the requested number of questions from mock pool
        const slicedMock = mockQuestions.slice(0, questionCount);
        setQuestions(slicedMock);
        setUserAnswers({});
        setCurrentQuestionIndex(0);
        setStep('quiz');
      }, 1200);
    } else {
      // Call the real C# Backend API
      const formData = new FormData();
      formData.append('File', file);
      formData.append('QuestionCount', questionCount.toString());

      try {
        const response = await fetch(BACKEND_URL, {
          method: 'POST',
          body: formData,
        });

        if (!response.ok) {
          const errMsg = await response.text();
          throw new Error(errMsg || `API returned status code ${response.status}`);
        }

        const data = await response.json();
        setQuestions(data);
        setUserAnswers({});
        setCurrentQuestionIndex(0);
        setStep('quiz');
      } catch (err) {
        console.error(err);
        setError(
          `Failed to connect to backend at ${BACKEND_URL}. Ensure the ASP.NET Core API is running, or toggle "Offline Mode" above to test immediately.`
        );
        setStep('setup');
      }
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

  const handleRestart = () => {
    setQuestions([]);
    setUserAnswers({});
    setCurrentQuestionIndex(0);
    setError('');
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
              offlineMode={offlineMode}
              setOfflineMode={setOfflineMode}
              onGenerate={handleGenerate}
              disabled={!file}
            />

            {error && (
              <div className="card error-card" style={{ borderLeft: '4px solid var(--danger)', marginTop: '24px' }}>
                <h4 style={{ color: 'var(--danger)', marginBottom: '8px', fontWeight: 600 }}>Connection Error</h4>
                <p style={{ fontSize: '0.9rem', color: 'var(--text-main)' }}>{error}</p>
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
            onRestart={handleRestart}
          />
        )}
      </main>

      <footer className="app-footer">
        <p>© {new Date().getFullYear()} PDF Quiz Generator. A modern learning prototype.</p>
      </footer>
    </div>
  );
}
