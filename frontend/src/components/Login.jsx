import { useState } from 'react';
import { apiService } from '../services/apiService';
import './Auth.css';

export function Login({ onLogin }) {
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await apiService.login(usernameOrEmail, password);
      const { token, user } = response.data;
      
      // Save token to localStorage
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
      
      setMessage('✓ Inloggad!');
      
      // Notify parent component
      if (onLogin) {
        onLogin(user);
      }
    } catch (error) {
      setMessage('✗ Inloggning misslyckades: ' + (error.response?.data || error.message));
    }
  };

  return (
    <div className="auth-container">
      <h2>Logga in</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Användarnamn eller email"
          value={usernameOrEmail}
          onChange={(e) => setUsernameOrEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Lösenord"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit">Logga in</button>
      </form>
      {message && <p className="message">{message}</p>}
    </div>
  );
}
