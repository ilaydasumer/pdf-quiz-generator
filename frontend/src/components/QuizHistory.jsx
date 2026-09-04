import React, { useState, useEffect } from 'react';
import QuizHistoryDetails from './QuizHistoryDetails';
import './QuizHistory.css';

function QuizHistory() {
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedQuizId, setSelectedQuizId] = useState(null);

  useEffect(() => {
    fetchHistory();
  }, []);

  const fetchHistory = async () => {
    setLoading(true);
    setError('');
    const token = localStorage.getItem('token');
    
    try {
      const response = await fetch('http://localhost:5292/api/quiz/history', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Failed to fetch quiz history.');
      }

      const data = await response.json();
      setHistory(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('tr-TR', options);
  };

  if (selectedQuizId) {
    return (
      <QuizHistoryDetails 
        quizId={selectedQuizId} 
        onBack={() => setSelectedQuizId(null)} 
      />
    );
  }

  return (
    <div className="card history-card">
      <div className="history-header">
        <h2>Test Geçmişiniz</h2>
        <button className="secondary-button" onClick={fetchHistory} disabled={loading}>
          Yenile
        </button>
      </div>

      {loading ? (
        <div className="history-loading">Yükleniyor...</div>
      ) : error ? (
        <div className="history-error">{error}</div>
      ) : history.length === 0 ? (
        <div className="history-empty">Henüz çözülmüş bir test bulunmuyor.</div>
      ) : (
        <div className="table-responsive">
          <table className="history-table">
            <thead>
              <tr>
                <th>Dosya Adı</th>
                <th>Zorluk</th>
                <th>Soru Sayısı</th>
                <th>Skor</th>
                <th>Tarih</th>
                <th>İşlem</th>
              </tr>
            </thead>
            <tbody>
              {history.map((record) => (
                <tr key={record.id}>
                  <td className="file-name" title={record.fileName}>{record.fileName}</td>
                  <td>
                    <span className={`badge badge-${record.difficulty.toLowerCase()}`}>
                      {record.difficulty}
                    </span>
                  </td>
                  <td>{record.totalQuestions}</td>
                  <td className="score-cell">
                    <strong>%{record.score}</strong>
                  </td>
                  <td>{formatDate(record.createdAt)}</td>
                  <td>
                    <button 
                      className="text-button" 
                      onClick={() => setSelectedQuizId(record.id)}
                    >
                      Detayları Gör
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default QuizHistory;
