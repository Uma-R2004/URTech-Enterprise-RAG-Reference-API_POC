/*
File role: app.component.ts
UI: Angular reference client
Purpose: Demonstrates a thin front end consuming the public .NET RAG API through a local development proxy.
Flow: User question -> /api/rag/ask -> grounded answer + sources -> UI rendering.
Design note: UI code is intentionally separate from the .NET layered backend so either client can be replaced independently.
*/

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RagAnswer, RagApiService } from './rag-api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  question = 'Who approves privileged access requests?';
  result?: RagAnswer;
  loading = false;
  error = '';

  constructor(private readonly ragApi: RagApiService) {}

  ask(): void {
    if (!this.question.trim()) return;
    this.loading = true;
    this.error = '';
    this.result = undefined;

    this.ragApi.ask(this.question).subscribe({
      next: response => {
        this.result = response.data;
        this.loading = false;
      },
      error: err => {
        this.error = err?.error?.message ?? 'Unable to call the RAG API.';
        this.loading = false;
      }
    });
  }
}
