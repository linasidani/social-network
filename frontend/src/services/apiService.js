import axios from 'axios';

const API_URL = 'http://127.0.0.1:5001/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const apiService = {
  // Users
  register: (username, email, password) =>
    api.post('/users/register', { username, email, password }),

  login: (usernameOrEmail, password) =>
    api.post('/users/login', { usernameOrEmail, password }),

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  },

  getUsers: () => api.get('/users'),
  
  getUser: (id) => api.get(`/users/${id}`),
  
  getUserByUsername: (username) => api.get(`/users/username/${username}`),

  // Posts
  createPost: (content, authorId, timelineOwnerId) =>
    api.post('/posts', { content, timelineOwnerId }, { params: { authorId } }),
  
  getPost: (id) => api.get(`/posts/${id}`),
  
  getUserPosts: (userId) => api.get(`/posts/user/${userId}`),
  
  getTimeline: (userId) => api.get(`/posts/timeline/${userId}`),
  getWall: (userId) => api.get(`/posts/wall/${userId}`),
  
  deletePost: (id) => api.delete(`/posts/${id}`),

  // Follows
  followUser: (followerId, followingId) =>
    api.post('/follows', null, { params: { followerId, followingId } }),
  
  unfollowUser: (followerId, followingId) =>
    api.delete('/follows', { params: { followerId, followingId } }),
  
  getFollowers: (userId) => api.get(`/follows/followers/${userId}`),
  
  getFollowing: (userId) => api.get(`/follows/following/${userId}`),
  
  getFollowersCount: (userId) => api.get(`/follows/${userId}/followers-count`),
  
  getFollowingCount: (userId) => api.get(`/follows/${userId}/following-count`),

  // Direct Messages
  sendMessage: (senderId, receiverId, content) =>
    api.post('/messages', { content, receiverId }, { params: { senderId } }),
  
  getMessage: (id) => api.get(`/messages/${id}`),
  
  getConversation: (userId1, userId2) =>
    api.get(`/messages/conversation/${userId1}/${userId2}`),
  
  getInbox: (userId) => api.get(`/messages/inbox/${userId}`),
  
  getSentMessages: (userId) => api.get(`/messages/sent/${userId}`),
};

export default api;
