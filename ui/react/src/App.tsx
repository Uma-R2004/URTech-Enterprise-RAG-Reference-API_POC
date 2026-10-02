/*
File role: App.tsx
UI: React reference client
Purpose: Demonstrates a thin front end consuming the public .NET RAG API through a local development proxy.
Flow: User question -> /api/rag/ask -> grounded answer + sources -> UI rendering.
Design note: UI code is intentionally separate from the .NET layered backend so either client can be replaced independently.
*/

import { useState } from 'react';

type RagSource = {
  documentId: string;
  chunkId: string;
  chunkSequence: number;
  similarityScore: number;
  text: string;
};

type RagAnswer = {
  question: string;
  answer: string;
  grounded: boolean;
  embeddingProvider: string;
  answerProvider: string;
  sources: RagSource[];
};

type ApiResponse<T> = { success: boolean; data: T; message?: string | null };

export default function App() {
  const [question, setQuestion] = useState('Who approves privileged access requests?');
  const [result, setResult] = useState<RagAnswer | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function ask() {
    if (!question.trim()) return;
    setLoading(true);
    setResult(null);
    setError('');

    try {
      const response = await fetch('/api/rag/ask', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ question, topK: 4, minimumSimilarityScore: 0.02 })
      });

      if (!response.ok) throw new Error(`API returned ${response.status}`);
      const payload = (await response.json()) as ApiResponse<RagAnswer>;
      setResult(payload.data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to call the RAG API.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="shell">
      <section className="hero">
        <p className="eyebrow">URTech Enterprise RAG Reference API</p>
        <h1>React RAG Client</h1>
        <p>Ask a question and inspect the evidence used to ground the answer.</p>
      </section>

      <section className="card">
        <label htmlFor="question">Question</label>
        <textarea id="question" rows={4} value={question} onChange={e => setQuestion(e.target.value)} />
        <button onClick={ask} disabled={loading}>{loading ? 'Searching…' : 'Ask RAG'}</button>
        {error && <p className="error">{error}</p>}
      </section>

      {result && (
        <section className="card">
          <div className="meta">
            <span>Grounded: {String(result.grounded)}</span>
            <span>Embedding: {result.embeddingProvider}</span>
            <span>Answer: {result.answerProvider}</span>
          </div>
          <h2>Answer</h2>
          <p>{result.answer}</p>
          <h2>Sources</h2>
          {result.sources.map(source => (
            <article className="source" key={source.chunkId}>
              <strong>Similarity {source.similarityScore}</strong>
              <p>{source.text}</p>
            </article>
          ))}
        </section>
      )}
    </main>
  );
}
