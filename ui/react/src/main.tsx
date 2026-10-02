/*
File role: main.tsx
UI: React reference client
Purpose: Demonstrates a thin front end consuming the public .NET RAG API through a local development proxy.
Flow: User question -> /api/rag/ask -> grounded answer + sources -> UI rendering.
Design note: UI code is intentionally separate from the .NET layered backend so either client can be replaced independently.
*/

import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './styles.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
