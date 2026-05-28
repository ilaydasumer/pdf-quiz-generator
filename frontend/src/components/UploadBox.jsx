import React, { useState, useRef } from 'react';

export default function UploadBox({ file, setFile }) {
  const [isDragActive, setIsDragActive] = useState(false);
  const [error, setError] = useState('');
  const fileInputRef = useRef(null);

  const handleDrag = (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setIsDragActive(true);
    } else if (e.type === 'dragleave') {
      setIsDragActive(false);
    }
  };

  const validateAndSetFile = (selectedFile) => {
    if (!selectedFile) return;

    if (selectedFile.type !== 'application/pdf' && !selectedFile.name.endsWith('.pdf')) {
      setError('Please select a valid PDF file.');
      return;
    }

    setError('');
    setFile(selectedFile);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      validateAndSetFile(e.dataTransfer.files[0]);
    }
  };

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files[0]) {
      validateAndSetFile(e.target.files[0]);
    }
  };

  const onButtonClick = () => {
    fileInputRef.current.click();
  };

  const clearFile = (e) => {
    e.stopPropagation();
    setFile(null);
    setError('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const formatBytes = (bytes, decimals = 2) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  };

  return (
    <div className="upload-container">
      <input
        ref={fileInputRef}
        type="file"
        id="input-file-upload"
        accept=".pdf,application/pdf"
        onChange={handleFileChange}
        style={{ display: 'none' }}
      />

      <div
        className={`upload-card ${isDragActive ? 'drag-active' : ''} ${file ? 'has-file' : ''}`}
        onDragEnter={handleDrag}
        onDragOver={handleDrag}
        onDragLeave={handleDrag}
        onDrop={handleDrop}
        onClick={!file ? onButtonClick : undefined}
      >
        {!file ? (
          <div className="upload-prompt">
            <div className="upload-icon">📄</div>
            <p className="primary-text">Drag & drop your PDF here, or <span className="browse-link">browse</span></p>
            <p className="secondary-text">Supports PDF documents up to 10MB</p>
            {error && <p className="error-message">{error}</p>}
          </div>
        ) : (
          <div className="file-info-container">
            <div className="file-icon">📄</div>
            <div className="file-details">
              <p className="file-name" title={file.name}>{file.name}</p>
              <p className="file-size">{formatBytes(file.size)}</p>
            </div>
            <button className="remove-file-btn" onClick={clearFile} aria-label="Remove file">
              ✕
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
