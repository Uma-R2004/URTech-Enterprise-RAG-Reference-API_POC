# Local Persistence and Uploaded Documents

The reference implementation is designed to be easy to run locally without cloud infrastructure.

## What happens when a document is uploaded

```text
Uploaded file
   -> API receives the stream
   -> text extractor reads supported content
   -> text normalizer cleans the extracted text
   -> chunker creates overlapping chunks
   -> embedding generator creates vectors
   -> vector store persists searchable chunks/vectors
   -> document repository persists document metadata
```

## Local storage locations

At runtime, the API creates an `App_Data` folder under the API project content root.

```text
src/URTech.EnterpriseRAG.Api/App_Data/
    documents.json
    vectors.json
```

### `documents.json`

Stores document metadata such as:

- document ID
- file name
- content type
- processing status
- character count
- chunk count
- created/processed timestamps

### `vectors.json`

Stores the searchable RAG records, including document/chunk relationships, chunk text and embedding vectors used for similarity search.

## Original uploaded file

The current public reference implementation **does not retain the original uploaded binary file** after text extraction. It persists metadata plus the searchable extracted/chunked/vectorized representation.

That is intentional for this lightweight public reference.

A production implementation can preserve the original document in Azure Blob Storage while using Azure AI Search, pgvector, or another approved vector store for retrieval.

```text
Original PDF/DOCX
      -> Azure Blob Storage
      -> extraction / OCR
      -> chunking
      -> embeddings
      -> vector search
      -> RAG
```

`App_Data` is excluded from source control so local runtime knowledge is not published to GitHub.
