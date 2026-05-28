import React from 'react';

export default function QuizQuestion({
  question,
  questionIndex,
  totalQuestions,
  selectedAnswer,
  onSelectAnswer,
  onPrev,
  onNext,
  onSubmit,
  isLast
}) {
  if (!question) return null;

  return (
    <div className="quiz-card">
      <div className="quiz-header">
        <span className="question-progress">Question {questionIndex + 1} of {totalQuestions}</span>
        <div className="progress-bar-container">
          <div 
            className="progress-bar-fill" 
            style={{ width: `${((questionIndex + 1) / totalQuestions) * 100}%` }}
          ></div>
        </div>
      </div>

      <h3 className="question-text">{question.questionText}</h3>

      <div className="options-list">
        {question.options.map((option) => {
          const isSelected = selectedAnswer === option.index;
          return (
            <button
              key={option.index}
              className={`option-card ${isSelected ? 'selected' : ''}`}
              onClick={() => onSelectAnswer(option.index)}
            >
              <span className="option-badge">
                {String.fromCharCode(65 + option.index)}
              </span>
              <span className="option-text">{option.text}</span>
            </button>
          );
        })}
      </div>

      <div className="quiz-navigation">
        <button
          className="nav-btn prev-btn"
          onClick={onPrev}
          disabled={questionIndex === 0}
        >
          ← Previous
        </button>

        {isLast ? (
          <button
            className="submit-btn"
            onClick={onSubmit}
            disabled={selectedAnswer === undefined}
          >
            Submit Quiz 🎉
          </button>
        ) : (
          <button
            className="nav-btn next-btn"
            onClick={onNext}
            disabled={selectedAnswer === undefined}
          >
            Next →
          </button>
        )}
      </div>
    </div>
  );
}
