import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { apiService } from '../services/apiService';
import './Feed.css';

export function Timeline() {
  const { userId } = useParams();
  const [user, setUser] = useState(null);
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [currentUser, setCurrentUser] = useState(null);

  useEffect(() => {
    const user = apiService.getCurrentUser();
    setCurrentUser(user);
  }, []);

  const loadUser = async (id) => {
    try {
      const response = await apiService.getUser(id);
      setUser(response.data);
    } catch (error) {
      console.error('Failed to load user:', error);
      setError('Kunde inte hämta användarinfo.');
    }
  };

  const loadTimeline = async (id) => {
    if (!id) return;

    try {
      setError('');
      setLoading(true);
      const response = await apiService.getTimeline(id);
      setPosts(response.data || []);
    } catch (error) {
      console.error('Failed to load timeline:', error);
      setError('Kunde inte hämta tidslinjen.');
      setPosts([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const targetUserId = userId || apiService.getCurrentUser()?.id;
    if (targetUserId) {
      loadUser(targetUserId);
      loadTimeline(targetUserId);
    }
  }, [userId]);

  if (!currentUser) {
    return (
      <div className="feed-container">
        <h2>Tidslinje</h2>
        <p>Du måste vara inloggad för att se tidslinjer. <a href="/login">Logga in här</a>.</p>
      </div>
    );
  }

  return (
    <div className="feed-container">
      <div className="feed-header">
        <div>
          <h2>{user ? `${user.username}s tidslinje` : 'Tidslinje'}</h2>
          <p>Se alla inlägg på användarens profil.</p>
        </div>
      </div>

      {error && <p className="error-message">{error}</p>}

      <div className="posts">
        {loading ? (
          <p>Laddar...</p>
        ) : posts.length > 0 ? (
          posts.map((post) => (
            <div key={post.id} className="post">
              <h4>{post.authorUsername}</h4>
              <p>{post.content}</p>
              <small>{new Date(post.createdAt).toLocaleString()}</small>
            </div>
          ))
        ) : (
          <p>Inga inlägg på denna tidslinje.</p>
        )}
      </div>
    </div>
  );
}
