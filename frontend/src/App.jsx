import './App.css'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { Register } from './components/Register'
import { Feed } from './components/Feed'
import { CreatePost } from './components/CreatePost'
import { Users } from './components/Users'
import { Messages } from './components/Messages'

function App() {
  return (
    <Router>
      <div className="app-shell">
        <header className="app-header">
          <div className="brand">Social Network</div>
          <nav>
            <Link to="/">Wall</Link>
            <Link to="/post">Post</Link>
            <Link to="/users">Users</Link>
            <Link to="/messages">Messages</Link>
            <Link to="/register">Register</Link>
          </nav>
        </header>

        <main className="app-main">
          <Routes>
            <Route path="/" element={<Feed />} />
            <Route path="/post" element={<CreatePost />} />
            <Route path="/users" element={<Users />} />
            <Route path="/messages" element={<Messages />} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </main>
      </div>
    </Router>
  )
}

export default App
