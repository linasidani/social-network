import axios from 'axios';

const API_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const apiService = {
  // Users
  register: (username, email, password) =>
    api.post('/users/register', { username, email, password }),
  
  getUsers: () => api.get('/users/all'),
  
  getUser: (id) => api.get(`/users/${id}`),
  
  getUserByUsername: (username) => api.get(`/users/username/${username}`),

  // Posts
  createPost: (content) => api.post('/posts/create', { content }),
  
  getPost: (id) => api.get(`/posts/${id}`),
  
  getUserPosts: (userId) => api.get(`/posts/user/${userId}`),
  
  getTimeline: (userId) => api.get(`/posts/timeline/${userId}`),
  
  deletePost: (id) => api.delete(`/posts/${id}`),

  // Follows
  followUser: (userId) => api.post('/follows/follow', { followedId: userId }),
  
  unfollowUser: (userId) => api.delete('/follows/unfollow', { data: { followedId: userId } }),
  
  getFollowers: (userId) => api.get(`/follows/followers/${userId}`),
  
  getFollowing: (userId) => api.get(`/follows/following/${userId}`),

  // Direct Messages
  sendMessage: (receiverId, content) => 
    api.post('/messages/send', { receiverId, content }),
  
  getMessage: (id) => api.get(`/messages/${id}`),
  
  getConversation: (userId1, userId2) => 
    api.get(`/messages/conversation/${userId1}/${userId2}`),
  
  getInbox: (userId) => api.get(`/messages/inbox/${userId}`),
};

export default api;
