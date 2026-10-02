# React RAG Client

Thin React/Vite client for **URTech Enterprise RAG Reference API**.

## Start

Keep the .NET API running at `https://localhost:7188`, then:

```powershell
npm install
npm run dev
```

Open `http://localhost:5173`.

The Vite development proxy forwards `/api` requests to the .NET backend. RAG orchestration remains centralized in the .NET Application layer.

## Verified result

![React RAG result](../../docs/images/06-react-rag-client.png)

See `../../docs/frontend/FRONTEND_TESTING.md` for the complete test flow.
