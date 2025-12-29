import { useEffect, useMemo, useState } from 'react'
import './App.css'

type MetalsLatestDto = {
  baseCurrency: string
  timestampUtc: string
  goldPerOunce: number
  silverPerOunce: number
  unit: string
}

function App() {
  const apiBaseUrl = useMemo(() => {
    const fromEnv = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim()
    if (fromEnv && fromEnv.length > 0) return fromEnv

    // Default to local API only during development.
    if (import.meta.env.DEV) return 'http://localhost:5080'

    // In production (e.g., GitHub Pages), require explicit configuration.
    return ''
  }, [])

  const [data, setData] = useState<MetalsLatestDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState<boolean>(true)

  useEffect(() => {
    let cancelled = false

    if (!apiBaseUrl) {
      setLoading(false)
      setError('Missing API base URL. Set VITE_API_BASE_URL (GitHub Actions secret recommended).')
      return
    }

    const load = async () => {
      try {
        setLoading(true)
        setError(null)

        const response = await fetch(`${apiBaseUrl}/api/metals/latest`)
        if (!response.ok) {
          const text = await response.text()
          throw new Error(`API ${response.status}: ${text || response.statusText}`)
        }

        const json = (await response.json()) as MetalsLatestDto
        if (!cancelled) setData(json)
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : String(e))
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    const id = window.setInterval(() => void load(), 10_000)
    return () => {
      cancelled = true
      window.clearInterval(id)
    }
  }, [apiBaseUrl])

  return (
    <>
      <h1>Metal Prices</h1>
      <p>Live gold and silver prices (updates every 10s).</p>

      {loading && <p>Loading…</p>}
      {error && (
        <p style={{ color: 'red' }}>
          {error}
        </p>
      )}

      {data && !error && (
        <div className="card">
          <p><strong>Gold</strong>: {data.goldPerOunce.toLocaleString()} {data.unit}</p>
          <p><strong>Silver</strong>: {data.silverPerOunce.toLocaleString()} {data.unit}</p>
          <p style={{ opacity: 0.8, fontSize: '0.9em' }}>
            Base: {data.baseCurrency} · Updated: {new Date(data.timestampUtc).toLocaleString()}
          </p>
        </div>
      )}
    </>
  )
}

export default App
