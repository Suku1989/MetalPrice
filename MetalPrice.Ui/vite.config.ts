import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  base: (() => {
    // For GitHub Pages, assets must be served from /<repo>/
    const repo = process.env.GITHUB_REPOSITORY?.split('/')?.[1]
    const isGhActions = process.env.GITHUB_ACTIONS === 'true'
    return (isGhActions && repo) ? `/${repo}/` : '/'
  })(),
  plugins: [react()],
})
