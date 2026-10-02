# Security

This public reference repository intentionally contains **no production credentials** and is designed so the default local RAG flow can run without cloud secrets.

## Never commit

- API keys
- passwords
- connection strings containing credentials
- access/refresh tokens
- private certificates
- tenant/client secrets
- `.env` or local secret files

## Recommended secret management

For real deployments use one or more of:

- .NET User Secrets for local development
- environment variables
- Azure Key Vault
- managed identity
- approved CI/CD secret variables

## AI/RAG controls demonstrated or documented

- Ground answers in retrieved enterprise text rather than unrestricted model knowledge
- Return source passages with RAG answers so evidence can be reviewed
- Fail safely when no supporting information meets retrieval criteria
- Keep cloud providers behind Application-layer abstractions
- Keep secrets out of source control
- Preserve the ability to add authentication, authorization, audit logging and content controls at the API boundary

## Public repository hygiene

The client-ready package removes `.git`, `.vs`, `bin`, `obj`, and runtime `App_Data` before publication.

## Reporting

If you identify a security issue in this reference implementation, report it privately to the repository owner rather than opening a public issue containing exploit details.
