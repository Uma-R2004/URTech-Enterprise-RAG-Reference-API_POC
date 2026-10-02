# Angular and React Client Testing

The repository includes two deliberately small front-end clients. Both call the same ASP.NET Core RAG backend and display the grounded answer together with source evidence.

## Prerequisite — start the .NET API first

1. Open `URTech.EnterpriseRAG.Reference.sln` in Visual Studio 2022.
2. Set `URTech.EnterpriseRAG.Api` as the startup project.
3. Rebuild the solution.
4. Start the `https` profile.
5. Confirm Swagger is available at `https://localhost:7188/swagger` when the default launch profile is unchanged.

Keep the API running while testing either UI.

---

## Angular client

Location:

```text
ui/angular
```

### Start Angular

```powershell
cd ui\angular
npm install
npm start
```

Open:

```text
http://localhost:4200
```

The Angular development proxy in `proxy.conf.json` forwards `/api` calls to:

```text
https://localhost:7188
```

### Angular verification scenario

1. Start the .NET API.
2. Start Angular.
3. Open `http://localhost:4200`.
4. Enter: `Who approves privileged access requests?`
5. Click **Ask RAG**.
6. Confirm that the screen displays:
   - a grounded answer,
   - the embedding provider,
   - the answer provider,
   - source passages,
   - similarity scores.

![Angular client result](../images/05-angular-rag-client.png)

---

## React client

Location:

```text
ui/react
```

### Start React

```powershell
cd ui\react
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The Vite proxy in `vite.config.ts` forwards `/api` calls to:

```text
https://localhost:7188
```

### React verification scenario

1. Keep the .NET API running.
2. Start React.
3. Open `http://localhost:5173`.
4. Enter: `Who approves privileged access requests?`
5. Click **Ask RAG**.
6. Confirm the answer and grounding evidence are displayed.

![React client result](../images/06-react-rag-client.png)

---

## Why both clients are included

The purpose is not to duplicate business logic. It demonstrates that the same enterprise API can support multiple front-end technologies while keeping RAG orchestration, retrieval, grounding, provider selection and persistence in the backend layers.

```text
Angular ----\
             >---- .NET 8 Web API ---- Application ---- Infrastructure/Persistence
React ------/
```
