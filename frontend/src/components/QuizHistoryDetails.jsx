import React, { useState, useEffect } from 'react';
import './QuizHistory.css';

function QuizHistoryDetails({ quizId, onBack }) {
  const [details, setDetails] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchDetails = async () => {
      const token = localStorage.getItem('token');
      try {
        const response = await fetch(`http://localhost:5292/api/quiz/history/${quizId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch details');
        }

        const data = await response.json();
        setDetails(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchDetails();
  }, [quizId]);

  if (loading) return <div className="history-loading">Detaylar yükleniyor...</div>;
  if (error) return <div className="history-error">{error}</div>;
  if (!details) return <div className="history-empty">Detay bulunamadı.</div>;

  return (
    <div className="card details-card">
      <div className="details-header">
        <button className="back-button" onClick={onBack}>
          ← Geri Dön
        </button>
        <h2>{details.fileName}</h2>
        <div className="details-meta">
          <span className={`badge badge-${details.difficulty.toLowerCase()}`}>{details.difficulty}</span>
          <span className="score-badge">Skor: %{details.score}</span>
        </div>
      </div>

      <div className="questions-review">
        {details.questions.map((q, qIndex) => {
          const isCorrect = q.userAnswerIndex === q.correctAnswerIndex;
          
          return (
            <div key={q.id} className={`review-question-card ${isCorrect ? 'correct-card' : 'incorrect-card'}`}>
              <div className="review-question-header">
                <span className="question-number">Soru {qIndex + 1}</span>
                <span className={`status-tag ${isCorrect ? 'status-correct' : 'status-incorrect'}`}>
                  {isCorrect ? 'Doğru' : 'Yanlış'}
                </span>
              </div>
              
              <h3 className="review-question-text">{q.questionText}</h3>
              
              <div className="review-options">
                {q.options.map((opt, oIndex) => {
                  let optClass = "review-option";
                  if (oIndex === q.correctAnswerIndex) {
                    optClass += " correct-option";
                  } else if (oIndex === q.userAnswerIndex && !isCorrect) {
                    optClass += " user-incorrect-option";
                  }

                  return (
                    <div key={oIndex} className={optClass}>
                      <span className="option-letter">
                        {String.fromCharCode(65 + oIndex)})
                      </span>
                      <span className="option-text">{opt}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default QuizHistoryDetails;
