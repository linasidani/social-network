import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiService } from '../services/apiService';
import './Feed.css';

export function Users() {
  const [users, setUsers] = useState([]);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [following, setFollowing] = useState([]);
  const [message, setMessage] = useState('');
  const [currentUser, setCurrentUser] = useState(null);

  useEffect(() => {
    const user = apiService.getCurrentUser();
    setCurrentUser(user);
  }, []);

  useEffect(() => {
    const loadUsers = async () => {
      try {
        const response = await apiService.getUsers();
        const userList = response.data || [];
        setUsers(userList);
        if (!currentUserId && userList.length > 0) {
          setCurrentUserId(userList[0].id);
        }
      } catch (error) {
        console.error('Failed to load users:', error);
      }
    };

    loadUsers();
  }, []);

  useEffect(() => {
    if (!currentUserId) return;

    const loadFollowing = async () => {
      try {
        const response = await apiService.getFollowing(currentUserId);
        setFollowing(response.data || []);
      } catch (error) {
        console.error('Failed to load following list:', error);
      }
    };

    loadFollowing();
  }, [currentUserId]);

  const handleFollow = async (userId) => {
    if (!currentUserId) return;
    try {
      await apiService.followUser(currentUserId, userId);
      setMessage('Följer nu användaren!');
      const response = await apiService.getFollowing(currentUserId);
      setFollowing(response.data || []);
    } catch (error) {
      console.error('Failed to follow user:', error);
      setMessage('Kunde inte följa användaren.');
    }
  };

  const handleUnfollow = async (userId) => {
    if (!currentUserId) return;
    try {
      await apiService.unfollowUser(currentUserId, userId);
      setMessage('Avföljde användaren!');
      const response = await apiService.getFollowing(currentUserId);
      setFollowing(response.data || []);
    } catch (error) {
      console.error('Failed to unfollow user:', error);
      setMessage('Kunde inte avfölja användaren.');
    }
  };

  const isFollowing = (userId) => following.some((user) => user.id === userId);

  if (!currentUser) {
    return (
      <div className="feed-container">
        <h2>Users</h2>
        <p>Du måste vara inloggad för att se användare. <a href="/login">Logga in här</a>.</p>
      </div>
    );
  }

  return (
    <div className="feed-container">
      <div className="feed-header">
        <div>
          <h2>Users</h2>
          <p>Follow other users to see their posts in your wall.</p>
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

      {message && <p className="message">{message}</p>}

      <div className="posts">
        {users.length > 0 ? (
          users.map((user) => (
            <div key={user.id} className="post">
              <h4>{user.username}</h4>
              <p>{user.email}</p>
              <div style={{ marginTop: '0.5rem' }}>
                <Link to={`/timeline/${user.id}`} style={{ marginRight: '0.5rem' }}>
                  Se tidslinje
                </Link>
                {currentUserId !== user.id && (
                  <button
                    type="button"
                    onClick={() => isFollowing(user.id) ? handleUnfollow(user.id) : handleFollow(user.id)}
                  >
                    {isFollowing(user.id) ? 'Avfölj' : 'Följ'}
                  </button>
                )}
              </div>
            </div>
          ))
        ) : (
          <p>No users available.</p>
        )}
      </div>
    </div>
  );
}
