import { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import './Feed.css';

export function CreatePost() {
  const [users, setUsers] = useState([]);
  const [currentUser, setCurrentUser] = useState(null);
  const [timelineOwnerId, setTimelineOwnerId] = useState('');
  const [content, setContent] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const user = apiService.getCurrentUser();
    setCurrentUser(user);

    const loadUsers = async () => {
      try {
        const response = await apiService.getUsers();
        setUsers(response.data || []);
      } catch (error) {
        console.error('Failed to load users:', error);
      }
    };

    loadUsers();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!currentUser) {
      setMessage('Du måste vara inloggad för att skapa inlägg.');
      return;
    }

    try {
      await apiService.createPost(content, currentUser.id, timelineOwnerId ? Number(timelineOwnerId) : null);
      setMessage('Post created successfully!');
      setContent('');
      setTimelineOwnerId('');
    } catch (error) {
      console.error('Create post failed:', error);
      setMessage('Failed to create post. Kontrollera inmatning och backend.');
    }
  };

  if (!currentUser) {
    return (
      <div className="feed-container">
        <h2>Create Post</h2>
        <p>Du måste vara inloggad för att skapa inlägg. <a href="/login">Logga in här</a>.</p>
      </div>
    );
  }

  return (
    <div className="feed-container">
      <h2>Create Post</h2>
      <p>Post a message as a user, optionally on another user's timeline.</p>

      <form className="post-form" onSubmit={handleSubmit}>
        <div className="author-display">
          <strong>Author:</strong> {currentUser.username}
        </div>

        <label>
          Timeline owner (optional)
          <select value={timelineOwnerId} onChange={(e) => setTimelineOwnerId(e.target.value)}>
            <option value="">Own timeline</option>
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
            rows={5}
            placeholder="Write your message"
            required
          />
        </label>

        <button type="submit">Post message</button>
      </form>

      {message && <p className="message">{message}</p>}
    </div>
  );
}
