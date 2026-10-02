/*
File role: vite.config.ts
UI: React reference client
Purpose: Demonstrates a thin front end consuming the public .NET RAG API through a local development proxy.
Flow: User question -> /api/rag/ask -> grounded answer + sources -> UI rendering.
Design note: UI code is intentionally separate from the .NET layered backend so either client can be replaced independently.
*/

import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7188',
        changeOrigin: true,
        secure: false
      }
    }
  }
});
