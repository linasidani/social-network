import { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import './Feed.css';

export function Feed() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const loadUsers = async () => {
    try {
      setError('');
      setLoading(true);
      const response = await apiService.getUsers();
      setUsers(response.data || []);
    } catch (error) {
      console.error('Failed to load users:', error);
      setError('Could not load users. Kontrollera att backend är igång.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  return (
    <div className="feed-container">
      <h2>Users</h2>
      <p>Browse social users registered in the app.</p>

      {error && <p className="error-message">{error}</p>}

      <div className="posts">
        {loading ? (
          <p>Loading...</p>
        ) : users.length > 0 ? (
          users.map((user) => (
            <div key={user.id} className="post">
              <h4>{user.username}</h4>
              <p>{user.email}</p>
              <small>{new Date(user.createdAt).toLocaleDateString()}</small>
            </div>
          ))
        ) : (
          <p>No users found yet.</p>
        )}
      </div>
    </div>
  );
}
