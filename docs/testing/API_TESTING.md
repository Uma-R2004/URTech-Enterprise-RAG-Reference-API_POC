# API Testing Guide

## 1. Start the API

1. Open `URTech.EnterpriseRAG.Reference.sln` in Visual Studio 2022.
2. Set `URTech.EnterpriseRAG.Api` as the startup project.
3. Rebuild the solution.
4. Start the `https` profile.
5. Swagger should open at `https://localhost:7188/swagger` when the default launch profile is unchanged.

Verified Swagger surface:

![Swagger endpoints](../images/08-swagger-endpoints-overview.png)

## 2. Health check

`GET /api/health`

Expected shape:

```json
{
  "status": "Healthy",
  "utc": "<UTC timestamp>",
  "embeddingProvider": "Local",
  "answerProvider": "Local",
  "indexedChunks": 1
}
```

The indexed chunk count varies as documents are added or removed.

## 3. List documents

`GET /api/documents`

A fresh local start seeds a small enterprise AI-governance policy so retrieval can be tested immediately.

After uploading a document, call the endpoint again and verify the new item appears.

![Indexed documents after upload](../images/11-indexed-documents-after-upload.png)

## 4. Ingest text

`POST /api/documents/ingest-text`

```json
{
  "fileName": "security-policy.txt",
  "content": "Production credentials must be stored in Azure Key Vault. Privileged access requires manager approval and security validation."
}
```

## 5. Upload a document

Use `POST /api/documents/upload` in Swagger and select a supported file.

Supported reference formats:

- `.txt`
- `.md`
- `.csv`
- `.json`
- `.html` / `.htm`
- text-based `.pdf`

A text-based PDF was verified locally:

![PDF upload success](../images/10-pdf-upload-success.png)

Scanned/image-only PDFs require OCR and are documented as a production extension point.

## 6. Ask a RAG question

`POST /api/rag/ask`

```json
{
  "question": "Who approves privileged access requests?",
  "topK": 4,
  "minimumSimilarityScore": 0.02
}
```

Validate that the response contains:

- the original question
- a grounded answer
- `grounded: true` when supporting content exists
- embedding and answer provider names
- one or more source passages
- similarity scores

Verified local result:

![Grounded RAG answer](../images/09-rag-api-grounded-answer.png)

## 7. Delete a document

Copy a document ID from `GET /api/documents` and call:

`DELETE /api/documents/{documentId}`

Expected result: HTTP 204.

## 8. Local persistence

The reference implementation persists local runtime state under:

```text
src/URTech.EnterpriseRAG.Api/App_Data/
    documents.json
    vectors.json
```

See [Local Persistence and Uploaded Documents](PERSISTENCE_AND_UPLOADS.md) for details.

## 9. Visual Studio `.http` file

`src/URTech.EnterpriseRAG.Api/URTech.EnterpriseRAG.Api.http` contains ready-to-run requests for the core API scenarios.
