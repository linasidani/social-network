import './App.css'
import { useState, useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { Register } from './components/Register'
import { Login } from './components/Login'
import { Feed } from './components/Feed'
import { CreatePost } from './components/CreatePost'
import { Users } from './components/Users'
import { Messages } from './components/Messages'
import { Timeline } from './components/Timeline'
import { apiService } from './services/apiService'

function App() {
  const [currentUser, setCurrentUser] = useState(null)

  useEffect(() => {
    // Check if user is already logged in
    const user = apiService.getCurrentUser()
    if (user) {
      setCurrentUser(user)
    }
  }, [])

  const handleLogin = (user) => {
    setCurrentUser(user)
  }

  const handleLogout = () => {
    apiService.logout()
    setCurrentUser(null)
  }

  return (
    <Router>
      <div className="app-shell">
        <header className="app-header">
          <div className="brand">Social Network</div>
          <nav>
            <Link to="/">Wall</Link>
            <Link to="/post">Post</Link>
            <Link to="/users">Users</Link>
            <Link to="/timeline">Min tidslinje</Link>
            <Link to="/messages">Messages</Link>
            {!currentUser && <Link to="/register">Registrera</Link>}
            {!currentUser && <Link to="/login">Logga in</Link>}
            {currentUser && (
              <span style={{ color: '#fff', marginLeft: '1rem' }}>
                {currentUser.username}
                <button onClick={handleLogout} style={{ marginLeft: '0.5rem' }}>
                  Logga ut
                </button>
              </span>
            )}
          </nav>
        </header>

        <main className="app-main">
          <Routes>
            <Route path="/" element={<Feed />} />
            <Route path="/post" element={<CreatePost />} />
            <Route path="/users" element={<Users />} />
            <Route path="/timeline/:userId" element={<Timeline />} />
            <Route path="/timeline" element={<Timeline />} />
            <Route path="/messages" element={<Messages />} />
            <Route path="/register" element={<Register />} />
            <Route path="/login" element={<Login onLogin={handleLogin} />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
