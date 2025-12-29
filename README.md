# MetalPrice

ASP.NET Core + React app that displays live gold and silver prices using MetalpriceAPI.

## Prereqs

- .NET SDK 10
- Node.js + npm
- A Metals-API key

## Configure MetalpriceAPI key

The backend reads the key from either:

- Environment variable: `METALPRICE_API_KEY` (recommended)
- Or `MetalPrice.Api/appsettings*.json` (`MetalpriceApi:ApiKey`)

Example (PowerShell):

```powershell
$env:METALPRICE_API_KEY = "YOUR_KEY_HERE"
```

Important: do not commit API keys to git. Use environment variables or GitHub Secrets.

## Run (development)

### 1) Start the API

```powershell
cd .\MetalPrice.Api
dotnet run
```

API runs on:

- https://localhost:7080

Endpoint:

- `GET https://localhost:7080/api/metals/latest`

### 2) Start the React UI

```powershell
cd .\MetalPrice.Ui
npm install
npm run dev
```

UI runs on:

- http://localhost:5173

The UI calls the API URL from `VITE_API_BASE_URL` (see `MetalPrice.Ui/.env.example`).

## Deploy UI to GitHub Pages

This repo deploys only the React UI to GitHub Pages via GitHub Actions.

- Workflow: [.github/workflows/deploy-ui-pages.yml](.github/workflows/deploy-ui-pages.yml)
- Set secret: `VITE_API_BASE_URL` to your deployed API base URL (example: `https://your-api-host`) in:
	- GitHub repo → Settings → Secrets and variables → Actions → New repository secret

Note: GitHub Pages cannot host the ASP.NET Core API. You must deploy the API separately.

## Notes

- The UI refreshes every 10 seconds.
- Prices are returned as `{baseCurrency} per oz` (computed by inverting the Metals-API rates).
