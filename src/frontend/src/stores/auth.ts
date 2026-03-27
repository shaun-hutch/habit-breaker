import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'

export interface User {
  id: string
  email: string
  preferredReminderTime: string | null
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const user = ref<User | null>(JSON.parse(localStorage.getItem('user') ?? 'null'))

  const isAuthenticated = computed(() => !!accessToken.value)

  function setTokens(access: string, refresh: string, u: User) {
    accessToken.value = access
    refreshToken.value = refresh
    user.value = u
    localStorage.setItem('accessToken', access)
    localStorage.setItem('refreshToken', refresh)
    localStorage.setItem('user', JSON.stringify(u))
  }

  async function login(email: string, password: string) {
    const res = await api.post('/api/auth/login', { email, password })
    setTokens(res.data.accessToken, res.data.refreshToken, res.data.user)
  }

  async function register(email: string, password: string) {
    const res = await api.post('/api/auth/register', { email, password })
    setTokens(res.data.accessToken, res.data.refreshToken, res.data.user)
  }

  async function refresh() {
    const token = refreshToken.value
    if (!token) throw new Error('No refresh token')
    const res = await api.post('/api/auth/refresh', { refreshToken: token })
    setTokens(res.data.accessToken, res.data.refreshToken, res.data.user)
  }

  async function logout() {
    try {
      if (refreshToken.value) {
        await api.post('/api/auth/logout', { refreshToken: refreshToken.value })
      }
    } catch {
      // Best-effort logout
    } finally {
      accessToken.value = null
      refreshToken.value = null
      user.value = null
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
    }
  }

  return { accessToken, refreshToken, user, isAuthenticated, login, register, refresh, logout }
})
