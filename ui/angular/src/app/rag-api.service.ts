/*
File role: rag-api.service.ts
UI: Angular reference client
Purpose: Demonstrates a thin front end consuming the public .NET RAG API through a local development proxy.
Flow: User question -> /api/rag/ask -> grounded answer + sources -> UI rendering.
Design note: UI code is intentionally separate from the .NET layered backend so either client can be replaced independently.
*/

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string | null;
}

export interface RagSource {
  documentId: string;
  chunkId: string;
  chunkSequence: number;
  similarityScore: number;
  text: string;
}

export interface RagAnswer {
  question: string;
  answer: string;
  grounded: boolean;
  embeddingProvider: string;
  answerProvider: string;
  sources: RagSource[];
}

@Injectable({ providedIn: 'root' })
export class RagApiService {
  constructor(private readonly http: HttpClient) {}

  ask(question: string): Observable<ApiResponse<RagAnswer>> {
    return this.http.post<ApiResponse<RagAnswer>>('/api/rag/ask', {
      question,
      topK: 4,
      minimumSimilarityScore: 0.02
    });
  }
}
