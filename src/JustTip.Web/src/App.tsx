import { useState, useEffect } from 'react'
import { RosterPage } from './pages/RosterPage'
import { TipsPage } from './pages/TipsPage'

type Page = 'roster' | 'tips'

function App() {
  const [currentPage, setCurrentPage] = useState<Page>(() => {
    const hash = window.location.hash.slice(1)
    return hash === 'tips' ? 'tips' : 'roster'
  })

  useEffect(() => {
    const handleHashChange = () => {
      const hash = window.location.hash.slice(1)
      setCurrentPage(hash === 'tips' ? 'tips' : 'roster')
    }

    window.addEventListener('hashchange', handleHashChange)
    return () => window.removeEventListener('hashchange', handleHashChange)
  }, [])

  return currentPage === 'tips' ? <TipsPage /> : <RosterPage />
}

export default App
