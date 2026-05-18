import { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import './Feed.css';

export function CreatePost() {
  const [users, setUsers] = useState([]);
  const [authorId, setAuthorId] = useState(null);
  const [timelineOwnerId, setTimelineOwnerId] = useState('');
  const [content, setContent] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const loadUsers = async () => {
      try {
        const response = await apiService.getUsers();
        const userList = response.data || [];
        setUsers(userList);
        if (userList.length > 0) {
          setAuthorId(userList[0].id);
        }
      } catch (error) {
        console.error('Failed to load users:', error);
      }
    };

    loadUsers();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!authorId) {
      setMessage('Select an author.');
      return;
    }

    try {
      await apiService.createPost(content, authorId, timelineOwnerId ? Number(timelineOwnerId) : null);
      setMessage('Post created successfully!');
      setContent('');
      setTimelineOwnerId('');
    } catch (error) {
      console.error('Create post failed:', error);
      setMessage('Failed to create post. Kontrollera inmatning och backend.');
    }
  };

  return (
    <div className="feed-container">
      <h2>Create Post</h2>
      <p>Post a message as a user, optionally on another user's timeline.</p>

      <form className="post-form" onSubmit={handleSubmit}>
        <label>
          Author
          <select value={authorId ?? ''} onChange={(e) => setAuthorId(Number(e.target.value))}>
            {users.map((user) => (
              <option value={user.id} key={user.id}>
                {user.username}
              </option>
            ))}
          </select>
        </label>

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
