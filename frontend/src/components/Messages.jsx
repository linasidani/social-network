import { useState, useEffect } from 'react'
import { apiService } from '../services/apiService'
import './Feed.css'

export function Messages() {
  const [users, setUsers] = useState([])
  const [currentUserId, setCurrentUserId] = useState(null)
  const [receiverId, setReceiverId] = useState(null)
  const [content, setContent] = useState('')
  const [inbox, setInbox] = useState([])
  const [sent, setSent] = useState([])
  const [message, setMessage] = useState('')
  const [currentUser, setCurrentUser] = useState(null)

  useEffect(() => {
    const user = apiService.getCurrentUser()
    setCurrentUser(user)
  }, [])

  useEffect(() => {
    const loadUsers = async () => {
      try {
        const response = await apiService.getUsers()
        const userList = response.data || []
        setUsers(userList)
        if (userList.length > 0) {
          setCurrentUserId(userList[0].id)
          setReceiverId(userList[1]?.id ?? userList[0].id)
        }
      } catch (error) {
        console.error('Failed to load users:', error)
      }
    }

    loadUsers()
  }, [])

  useEffect(() => {
    if (!currentUserId) return

    const loadMessages = async () => {
      try {
        const inboxResponse = await apiService.getInbox(currentUserId)
        setInbox(inboxResponse.data || [])

        const sentResponse = await apiService.getSentMessages(currentUserId)
        setSent(sentResponse.data || [])
      } catch (error) {
        console.error('Failed to load messages:', error)
      }
    }

    loadMessages()
  }, [currentUserId])

  const handleSend = async (e) => {
    e.preventDefault()

    if (!currentUserId || !receiverId) {
      setMessage('Select sender and receiver.')
      return
    }

    if (!content.trim()) {
      setMessage('Message content is required.')
      return
    }

    try {
      await apiService.sendMessage(currentUserId, receiverId, content)
      setMessage('Message sent!')
      setContent('')
      const inboxResponse = await apiService.getInbox(currentUserId)
      setInbox(inboxResponse.data || [])
      const sentResponse = await apiService.getSentMessages(currentUserId)
      setSent(sentResponse.data || [])
    } catch (error) {
      console.error('Send message failed:', error)
      setMessage('Could not send message. Kontrollera backend.')
    }
  }

  if (!currentUser) {
    return (
      <div className="feed-container">
        <h2>Messages</h2>
        <p>Du måste vara inloggad för att se meddelanden. <a href="/login">Logga in här</a>.</p>
      </div>
    )
  }

  return (
    <div className="feed-container">
      <h2>Direct Messages</h2>
      <p>Send and receive private messages between users.</p>

      <form className="post-form" onSubmit={handleSend}>
        <label>
          Sender
          <select value={currentUserId ?? ''} onChange={(e) => setCurrentUserId(Number(e.target.value))}>
            {users.map((user) => (
              <option value={user.id} key={user.id}>
                {user.username}
              </option>
            ))}
          </select>
        </label>

        <label>
          Receiver
          <select value={receiverId ?? ''} onChange={(e) => setReceiverId(Number(e.target.value))}>
            {users.map((user) => (
              <option value={user.id} key={user.id}>
                {user.username}
              </option>
            ))}
          </select>
        </label>

        <label>
          Message
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={4}
            placeholder="Write a direct message"
            required
          />
        </label>

        <button type="submit">Send DM</button>
      </form>

      {message && <p className="message">{message}</p>}

      <div className="posts">
        <div className="post">
          <h4>Inbox</h4>
          {inbox.length === 0 ? (
            <p>No received messages.</p>
          ) : (
            inbox.map((msg) => (
              <div key={msg.id} className="post">
                <strong>From: {msg.senderUsername}</strong>
                <p>{msg.content}</p>
                <small>{new Date(msg.createdAt).toLocaleString()}</small>
              </div>
            ))
          )}
        </div>

        <div className="post">
          <h4>Sent</h4>
          {sent.length === 0 ? (
            <p>No sent messages.</p>
          ) : (
            sent.map((msg) => (
              <div key={msg.id} className="post">
                <strong>To: {msg.receiverUsername}</strong>
                <p>{msg.content}</p>
                <small>{new Date(msg.createdAt).toLocaleString()}</small>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  )
}
