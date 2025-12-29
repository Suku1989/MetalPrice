# MetalPrice

ASP.NET Core + React app that displays live gold and silver prices using MetalpriceAPI.

## Prereqs

- .NET SDK 10
- Node.js + npm
- A MetalpriceAPI key

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
- GitHub Pages settings:
	- GitHub repo → Settings → Pages → Build and deployment → Source: **Deploy from a branch**
	- Branch: `gh-pages` / Folder: `/ (root)`
- Set secret: `VITE_API_BASE_URL` to your deployed API base URL (example: `https://your-api-host`) in:
	- GitHub repo → Settings → Secrets and variables → Actions → New repository secret

Note: GitHub Pages cannot host the ASP.NET Core API. You must deploy the API separately.

## Deploy API to Azure App Service

This repo includes a GitHub Actions workflow that deploys the ASP.NET Core API to Azure App Service:

- Workflow: [.github/workflows/deploy-api-azure.yml](.github/workflows/deploy-api-azure.yml)

### 1) Create Azure resources

In Azure Portal:

- Create an **App Service** (Runtime: `.NET 10` if available; otherwise the closest supported .NET version for App Service).

### 2) Add GitHub Secrets

In GitHub repo → Settings → Secrets and variables → Actions:

- `AZURE_WEBAPP_NAME`: your App Service *resource name* (the “Name” shown in Azure Portal → App Service → Overview)
- `AZURE_WEBAPP_PUBLISH_PROFILE`: download from Azure Portal → App Service → **Get publish profile** (paste the full XML into the secret)

Note: the App Service **resource name** can differ from the **default hostname** (the `https://...azurewebsites.net` URL). Use:

- `AZURE_WEBAPP_NAME` = App Service resource name
- `VITE_API_BASE_URL` = App Service hostname (base URL)

### 3) Configure App Settings in Azure

In Azure Portal → App Service → Configuration → Application settings:

- `METALPRICE_API_KEY`: your MetalpriceAPI key
- `Cors__AllowedOrigins__0`: your GitHub Pages origin, e.g. `https://suku1989.github.io`

Then set the UI secret:

- GitHub secret `VITE_API_BASE_URL`: `https://<your-app-service-hostname>`

## Notes

- The UI refreshes every 10 seconds.
- Prices are returned as `{baseCurrency} per oz` (computed by inverting the Metals-API rates).

## Production checklist

Use this checklist when setting up a new repo/environment.

### GitHub Secrets (repo → Settings → Secrets and variables → Actions)

- `VITE_API_BASE_URL`: your deployed API base URL (example: `https://<your-app>.azurewebsites.net`)
- `AZURE_WEBAPP_NAME`: your App Service **resource name** (example: `MetalPrice`)
- `AZURE_WEBAPP_PUBLISH_PROFILE`: publish profile XML for that exact App Service (Azure Portal → App Service → Get publish profile)
- Ensure `AZURE_WEBAPP_SLOT_NAME` is **not set** unless you use deployment slots

### Azure App Service App Settings (Azure Portal → App Service → Configuration)

- `METALPRICE_API_KEY`: your MetalpriceAPI key
- `Cors__AllowedOrigins__0`: your GitHub Pages origin (example: `https://suku1989.github.io`)

### GitHub Pages

- Repo → Settings → Pages → Source: **Deploy from a branch**
- Branch: `gh-pages` / Folder: `/ (root)`

### Verify

- API: `GET https://<your-app>.azurewebsites.net/api/metals/latest`
- UI: `https://<your-gh-user>.github.io/MetalPrice/`
