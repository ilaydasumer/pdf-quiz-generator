import React from 'react';

export default function QuizSettings({ 
  questionCount, 
  setQuestionCount, 
  offlineMode, 
  setOfflineMode, 
  onGenerate, 
  disabled 
}) {
  const counts = [5, 10, 15];

  return (
    <div className="settings-card">
      <div className="settings-group">
        <label className="settings-label">Number of Questions</label>
        <div className="btn-group">
          {counts.map((count) => (
            <button
              key={count}
              type="button"
              className={`count-btn ${questionCount === count ? 'active' : ''}`}
              onClick={() => setQuestionCount(count)}
              disabled={disabled}
            >
              {count} Questions
            </button>
          ))}
        </div>
      </div>

      <div className="settings-group offline-toggle-container">
        <label className="switch-label">
          <input
            type="checkbox"
            checked={offlineMode}
            onChange={(e) => setOfflineMode(e.target.checked)}
            disabled={disabled}
          />
          <span className="checkbox-text">Use Local Mock Questions (Offline Mode)</span>
        </label>
        <p className="toggle-help-text">
          {offlineMode 
            ? "Generates quiz immediately using client-side mock questions." 
            : "Sends the PDF to the ASP.NET Core API at http://localhost:5070."}
        </p>
      </div>

      <button
        className="generate-btn"
        onClick={onGenerate}
        disabled={disabled}
      >
        {disabled ? 'Generating Quiz...' : '⚡ Generate Quiz'}
      </button>
    </div>
  );
}
