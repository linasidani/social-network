import './App.css'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { Register } from './components/Register'
import { Feed } from './components/Feed'

function App() {
  return (
    <Router>
      <div className="app-shell">
        <header className="app-header">
          <div className="brand">Social Network</div>
          <nav>
            <Link to="/">Feed</Link>
            <Link to="/register">Register</Link>
          </nav>
        </header>

        <main className="app-main">
          <Routes>
            <Route path="/" element={<Feed />} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
