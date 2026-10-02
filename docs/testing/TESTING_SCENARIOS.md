# Testing Scenarios

The repository contains automated unit tests and manual end-to-end scenarios covering the local RAG workflow, document ingestion, persistence, Swagger, Angular and React.

## Automated tests

Run **Test → Run All Tests** in Visual Studio 2022 or run `dotnet test` from the repository root.

| Test area | What is validated |
|---|---|
| Chunking | Text is split into bounded, overlapping chunks while preserving context |
| Local embeddings | The deterministic local embedding provider returns repeatable vectors |
| RAG flow | A question is embedded, matched to indexed content and returns a grounded answer with sources |

## Backend/API test matrix

| ID | Scenario | Endpoint / Action | Expected result |
|---|---|---|---|
| TS-01 | Build solution | Visual Studio Rebuild | All projects compile successfully |
| TS-02 | Health check | `GET /api/health` | HTTP 200 and provider/runtime information |
| TS-03 | List seeded documents | `GET /api/documents` | HTTP 200 and seeded policy returned |
| TS-04 | Ingest raw text | `POST /api/documents/ingest-text` | HTTP 200; Ready status and chunks created |
| TS-05 | Upload text file | `POST /api/documents/upload` | HTTP 200 and document indexed |
| TS-06 | Upload text-based PDF | `POST /api/documents/upload` | HTTP 200 when readable text exists |
| TS-07 | Upload unsupported file | `POST /api/documents/upload` | Clear unsupported-file error |
| TS-08 | Ask grounded question | `POST /api/rag/ask` | HTTP 200; grounded answer and source passages |
| TS-09 | Ask unrelated question | `POST /api/rag/ask` | Safe insufficient-evidence response when retrieval is weak |
| TS-10 | Restrict to document | `POST /api/rag/ask` with `documentId` | Retrieval limited to requested document |
| TS-11 | Delete document | `DELETE /api/documents/{documentId}` | HTTP 204; metadata/vectors removed |
| TS-12 | Blank question | `POST /api/rag/ask` | HTTP 400 validation response |
| TS-13 | Empty upload | `POST /api/documents/upload` | HTTP 400 |
| TS-14 | Top-K bounds | TopK above supported maximum | Value is clamped to supported limit |
| TS-15 | Similarity bounds | Score outside supported range | Value is clamped to valid range |
| TS-16 | Local zero-secret startup | Default configuration | API starts without cloud credentials |
| TS-17 | Missing Azure secrets | Select Azure providers without required config | Clear startup/configuration failure |

## Front-end test matrix

| ID | Scenario | Client | Expected result |
|---|---|---|---|
| UI-01 | Angular startup | `npm start` in `ui/angular` | Application available on `localhost:4200` |
| UI-02 | Angular RAG question | Click **Ask RAG** | Grounded answer, sources and similarity scores displayed |
| UI-03 | React startup | `npm run dev` in `ui/react` | Application available on `localhost:5173` |
| UI-04 | React RAG question | Click **Ask RAG** | Grounded answer, sources and similarity scores displayed |
| UI-05 | Shared backend | Run Angular and React together | Both clients call the same .NET API through local proxies |

## Verified local evidence

The following scenarios were captured from the working local application.

### .NET solution build

**7 projects succeeded, 0 failed, 0 warnings, 0 errors.**

![Build success](../images/01-build-success.png)

### Swagger endpoint surface

![Swagger API overview](../images/08-swagger-endpoints-overview.png)

### PDF upload/indexing

![PDF upload success](../images/10-pdf-upload-success.png)

### Indexed documents after upload

![Indexed documents](../images/11-indexed-documents-after-upload.png)

### RAG grounded answer with evidence

![RAG result](../images/09-rag-api-grounded-answer.png)

### Angular result

![Angular result](../images/05-angular-rag-client.png)

### React result

![React result](../images/06-react-rag-client.png)

## Recommended RAG questions

For the seeded governance policy:

1. Who approves privileged access requests?
2. Where should production secrets be stored?
3. What reviews happen before a generative AI application is promoted to production?
4. Why should retrieved passages be preserved?
5. What happens after a significant AI incident?

For uploaded sample documents, ask questions whose answers appear explicitly in those files so grounding and source selection can be verified.

For the complete runtime evidence, see [Verified Local Results](VERIFIED_LOCAL_RESULTS.md).
