import React, { useState } from 'react';
import UploadBox from './components/UploadBox';
import QuizSettings from './components/QuizSettings';
import QuizQuestion from './components/QuizQuestion';
import ResultScreen from './components/ResultScreen';
import { mockQuestions } from './data/mockQuestions';
import Login from './components/Login';
import Register from './components/Register';
import QuizHistory from './components/QuizHistory';

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
  
  const [authStep, setAuthStep] = useState('login'); // 'login' | 'register'
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
  const [userEmail, setUserEmail] = useState(localStorage.getItem('userEmail') || '');
  const [activeTab, setActiveTab] = useState('generator'); // 'generator' | 'history'

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

  const handleSubmit = async () => {
    // Validate that all questions are answered
    if (Object.keys(userAnswers).length < questions.length) {
      alert('Please answer all questions before submitting.');
      return;
    }
    
    // Save to Database History if authenticated
    if (isAuthenticated) {
      // Calculate score percentage
      let correctCount = 0;
      questions.forEach((q, index) => {
        if (userAnswers[index] === q.correctAnswerIndex) {
          correctCount++;
        }
      });
      const scorePercent = Math.round((correctCount / questions.length) * 100);

      try {
        const token = localStorage.getItem('token');
        const payload = {
          fileName: file ? file.name : (offlineMode ? "Mock_Quiz.pdf" : "Generated_Quiz.pdf"),
          difficulty: difficulty,
          score: scorePercent,
          questions: questions.map((q, idx) => ({
            questionText: q.questionText,
            correctAnswerIndex: q.correctAnswerIndex,
            userAnswerIndex: userAnswers[idx],
            options: q.options.map(o => o.text)
          }))
        };

        const response = await fetch('http://localhost:5292/api/quiz/history', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });

        if (!response.ok) {
          console.error("Failed to save history record.");
        }
      } catch (err) {
        console.error("Error saving history:", err);
      }
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

  const handleLoginSuccess = (token, email) => {
    setIsAuthenticated(true);
    setUserEmail(email);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userEmail');
    setIsAuthenticated(false);
    setUserEmail('');
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <span className="app-logo">📝</span>
        <h1 className="app-title">PDF Quiz Generator</h1>
        <p className="app-description">
          Upload a study guide, textbook chapter, or article in PDF format, choose your options, and generate a customized quiz instantly.
        </p>
        {isAuthenticated && (
          <div className="auth-user-info">
            <span>Logged in as <strong>{userEmail}</strong></span>
            <button className="logout-button" onClick={handleLogout}>Log Out</button>
          </div>
        )}
      </header>

      {isAuthenticated && (
        <div className="app-nav-tabs">
          <button 
            className={`nav-tab ${activeTab === 'generator' ? 'active' : ''}`}
            onClick={() => setActiveTab('generator')}
          >
            Generate Quiz
          </button>
          <button 
            className={`nav-tab ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            Quiz History
          </button>
        </div>
      )}

      <main className="app-content">
        {!isAuthenticated ? (
          authStep === 'login' ? (
            <Login onLoginSuccess={handleLoginSuccess} switchToRegister={() => setAuthStep('register')} />
          ) : (
            <Register switchToLogin={() => setAuthStep('login')} />
          )
        ) : activeTab === 'history' ? (
          <QuizHistory />
        ) : (
          <>
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
        </>
        )}
      </main>

      <footer className="app-footer">
        <p>© {new Date().getFullYear()} PDF Quiz Generator. A modern learning prototype.</p>
      </footer>
    </div>
  );
}
