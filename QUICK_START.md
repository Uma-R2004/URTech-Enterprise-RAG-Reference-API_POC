# Quick Start — URTech Enterprise RAG Reference API

This guide starts the verified local configuration used for the screenshots in this repository.

## 1. Start the .NET 8 backend

1. Extract the repository to a short local path such as `C:\GH\URTechAIRAG`.
2. Open `URTech.EnterpriseRAG.Reference.sln` in Visual Studio 2022.
3. Set `URTech.EnterpriseRAG.Api` as the startup project.
4. Select **Build → Rebuild Solution**.
5. Select **Test → Run All Tests**.
6. Start the API using the `https` profile.
7. Open Swagger at `https://localhost:7188/swagger` when the default launch profile is unchanged.

The backend defaults to **Local mode** and requires no Azure/OpenAI secret.

Verified build evidence:

![Build success](docs/images/01-build-success.png)

## 2. Verify document APIs

Run:

`GET /api/documents`

Then upload a supported document using:

`POST /api/documents/upload`

A locally tested text-based PDF upload returned HTTP 200 and was indexed successfully:

![PDF upload success](docs/images/10-pdf-upload-success.png)

The uploaded document then appeared in `GET /api/documents`:

![Indexed documents](docs/images/11-indexed-documents-after-upload.png)

## 3. Test the RAG endpoint

Run:

```json
POST /api/rag/ask
{
  "question": "Who approves privileged access requests?",
  "topK": 4,
  "minimumSimilarityScore": 0.02
}
```

The verified local response returned HTTP 200, `grounded: true`, source passages and similarity scores:

![RAG answer](docs/images/09-rag-api-grounded-answer.png)

## 4. Start the Angular client

Keep the .NET API running.

```powershell
cd ui\angular
npm install
npm start
```

Open:

```text
http://localhost:4200
```

The Angular development proxy forwards `/api` to the .NET API at `https://localhost:7188`.

Verified Angular result:

![Angular RAG client](docs/images/05-angular-rag-client.png)

## 5. Start the React client

Open a second terminal while the API remains running:

```powershell
cd ui\react
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The Vite development proxy forwards `/api` to the same .NET backend.

Verified React result:

![React RAG client](docs/images/06-react-rag-client.png)

## 6. What runs together

```text
Swagger                    https://localhost:7188/swagger
Angular                    http://localhost:4200
React                      http://localhost:5173
                           |
                           v
                    ASP.NET Core API
                           |
                           v
       ingestion -> chunking -> embeddings -> retrieval -> RAG
```

For more detail, see:

- `docs/testing/VERIFIED_LOCAL_RESULTS.md`
- `docs/testing/TESTING_SCENARIOS.md`
- `docs/frontend/FRONTEND_TESTING.md`
- `docs/testing/PERSISTENCE_AND_UPLOADS.md`
