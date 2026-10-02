# Angular RAG Client

Thin Angular client for **URTech Enterprise RAG Reference API**.

## Start

Keep the .NET API running at `https://localhost:7188`, then:

```powershell
npm install
npm start
```

Open `http://localhost:4200`.

The development proxy forwards `/api` requests to the .NET backend. RAG orchestration remains in the backend Application layer; the Angular application only collects the user question and renders the answer/evidence.

## Verified result

![Angular RAG result](../../docs/images/05-angular-rag-client.png)

See `../../docs/frontend/FRONTEND_TESTING.md` for the complete test flow.
