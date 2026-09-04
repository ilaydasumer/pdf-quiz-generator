import React from 'react';

export default function ResultScreen({ questions, userAnswers, onRestart, onNewUpload }) {
  // Calculate score
  let correctCount = 0;
  questions.forEach((q, index) => {
    if (userAnswers[index] === q.correctAnswerIndex) {
      correctCount++;
    }
  });

  const totalQuestions = questions.length;
  const scorePercent = Math.round((correctCount / totalQuestions) * 100);

  // Pass/fail message
  let feedbackMessage = '';
  let feedbackClass = '';
  const passed = scorePercent >= 50;

  if (scorePercent === 100) {
    feedbackMessage = 'PASS - Perfect Score! 🌟';
    feedbackClass = 'feedback-perfect';
  } else if (passed) {
    feedbackMessage = 'PASS - Great job! 👍';
    feedbackClass = 'feedback-great';
  } else {
    feedbackMessage = 'FAIL - Keep practicing! 💪';
    feedbackClass = 'feedback-poor';
  }

  return (
    <div className="results-container">
      <div className="results-card">
        <h2 className="results-title">Quiz Results</h2>
        <div className={`score-badge ${feedbackClass}`}>
          <div className="score-number">{correctCount} / {totalQuestions}</div>
          <div className="score-percentage">{scorePercent}%</div>
        </div>
        <p className={`results-feedback ${feedbackClass}`}>{feedbackMessage}</p>
        <div className="result-actions">
          <button className="restart-btn" onClick={onRestart}>
            🔄 Try Again
          </button>
          <button className="new-upload-btn" onClick={onNewUpload}>
            📄 Upload Another PDF
          </button>
        </div>
      </div>

      <div className="review-section">
        <h3 className="review-title">Detailed Answer Review</h3>
        <div className="review-list">
          {questions.map((q, qIdx) => {
            const userAnswer = userAnswers[qIdx];
            const isCorrect = userAnswer === q.correctAnswerIndex;

            return (
              <div key={q.id || qIdx} className={`review-card ${isCorrect ? 'correct' : 'incorrect'}`}>
                <div className="review-header">
                  <span className="review-index">Question {qIdx + 1}</span>
                  <span className={`status-pill ${isCorrect ? 'correct' : 'incorrect'}`}>
                    {isCorrect ? '✓ Correct' : '✗ Incorrect'}
                  </span>
                </div>
                
                <h4 className="review-question">{q.questionText}</h4>
                
                <div className="review-options">
                  {q.options.map((opt) => {
                    const isOptSelected = userAnswer === opt.index;
                    const isOptCorrect = q.correctAnswerIndex === opt.index;
                    
                    let optClass = '';
                    if (isOptCorrect) optClass = 'opt-correct';
                    else if (isOptSelected && !isOptCorrect) optClass = 'opt-incorrect';

                    return (
                      <div key={opt.index} className={`review-option ${optClass}`}>
                        <span className="opt-indicator">
                          {isOptCorrect && '✓'}
                          {!isOptCorrect && isOptSelected && '✗'}
                          {!isOptCorrect && !isOptSelected && String.fromCharCode(65 + opt.index)}
                        </span>
                        <span className="opt-text">{opt.text}</span>
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
