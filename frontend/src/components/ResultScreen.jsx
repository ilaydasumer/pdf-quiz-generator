import React from 'react';

export default function ResultScreen({ questions, userAnswers, onRestart }) {
  // Calculate score
  let correctCount = 0;
  questions.forEach((q, index) => {
    if (userAnswers[index] === q.correctAnswerIndex) {
      correctCount++;
    }
  });

  const totalQuestions = questions.length;
  const scorePercent = Math.round((correctCount / totalQuestions) * 100);

  // Motivational feedback based on score
  let feedbackMessage = '';
  let feedbackClass = '';
  if (scorePercent === 100) {
    feedbackMessage = 'Perfect Score! You nailed it! 🌟';
    feedbackClass = 'feedback-perfect';
  } else if (scorePercent >= 80) {
    feedbackMessage = 'Great job! You have a solid understanding. 👍';
    feedbackClass = 'feedback-great';
  } else if (scorePercent >= 50) {
    feedbackMessage = 'Not bad! Review the answers below to improve. 📖';
    feedbackClass = 'feedback-medium';
  } else {
    feedbackMessage = 'Keep practicing! Give it another try to boost your score. 💪';
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
        <p className="results-feedback">{feedbackMessage}</p>
        <button className="restart-btn" onClick={onRestart}>
          🔄 Try Another Quiz
        </button>
      </div>

      <div className="review-section">
        <h3 className="review-title">Question Breakdown</h3>
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
