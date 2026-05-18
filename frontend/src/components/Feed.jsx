import { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import './Feed.css';

export function Feed() {
  const [users, setUsers] = useState([]);
  const [wall, setWall] = useState([]);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [currentUser, setCurrentUser] = useState(null);

  useEffect(() => {
    const user = apiService.getCurrentUser();
    setCurrentUser(user);
  }, []);

  const loadUsers = async () => {
    try {
      setError('');
      const response = await apiService.getUsers();
      const userList = response.data || [];
      setUsers(userList);
      if (!currentUserId && userList.length > 0) {
        setCurrentUserId(userList[0].id);
      }
    } catch (error) {
      console.error('Failed to load users:', error);
      setError('Could not load users. Kontrollera att backend är igång.');
    }
  };

  const loadWall = async (userId) => {
    if (!userId) return;

    try {
      setError('');
      setLoading(true);
      const response = await apiService.getWall(userId);
      setWall(response.data || []);
    } catch (error) {
      console.error('Failed to load wall:', error);
      setError('Could not load wall. Kontrollera att backend är igång.');
      setWall([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  useEffect(() => {
    if (currentUserId) {
      loadWall(currentUserId);
    }
  }, [currentUserId]);

  if (!currentUser) {
    return (
      <div className="feed-container">
        <h2>Wall</h2>
        <p>Du måste vara inloggad för att se Wall. <a href="/login">Logga in här</a>.</p>
      </div>
    );
  }

  return (
    <div className="feed-container">
      <div className="feed-header">
        <div>
          <h2>Wall</h2>
          <p>See user posts from people you follow.</p>
        </div>
        <div className="user-select">
          <label htmlFor="currentUser">Current user</label>
          <select
            id="currentUser"
            value={currentUserId ?? ''}
            onChange={(e) => setCurrentUserId(Number(e.target.value))}
          >
            {users.map((user) => (
              <option value={user.id} key={user.id}>
                {user.username}
              </option>
            ))}
          </select>
        </div>
      </div>

      {error && <p className="error-message">{error}</p>}

      <div className="posts">
        {loading ? (
          <p>Loading...</p>
        ) : wall.length > 0 ? (
          wall.map((post) => (
            <div key={post.id} className="post">
              <h4>{post.authorUsername}</h4>
              <p>{post.content}</p>
              <small>{new Date(post.createdAt).toLocaleString()}</small>
            </div>
          ))
        ) : (
          <p>No posts found. Create a post or follow someone.</p>
        )}
      </div>
    </div>
  );
}
