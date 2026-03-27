import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'

// Mock the api module
vi.mock('@/services/api', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() }
    }
  }
}))

import api from '@/services/api'

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('starts unauthenticated when no token in storage', () => {
    const auth = useAuthStore()
    expect(auth.isAuthenticated).toBe(false)
  })

  it('sets authenticated state after login', async () => {
    const mockUser = { id: 'u1', email: 'test@test.com', preferredReminderTime: null }
    vi.mocked(api.post).mockResolvedValueOnce({
      data: { accessToken: 'jwt', refreshToken: 'refresh', user: mockUser }
    })

    const auth = useAuthStore()
    await auth.login('test@test.com', 'password')

    expect(auth.isAuthenticated).toBe(true)
    expect(auth.accessToken).toBe('jwt')
    expect(auth.user?.email).toBe('test@test.com')
  })

  it('stores tokens in localStorage after login', async () => {
    const mockUser = { id: 'u1', email: 'test@test.com', preferredReminderTime: null }
    vi.mocked(api.post).mockResolvedValueOnce({
      data: { accessToken: 'jwt', refreshToken: 'refresh', user: mockUser }
    })

    const auth = useAuthStore()
    await auth.login('test@test.com', 'password')

    expect(localStorage.getItem('accessToken')).toBe('jwt')
    expect(localStorage.getItem('refreshToken')).toBe('refresh')
  })

  it('clears state after logout', async () => {
    const mockUser = { id: 'u1', email: 'test@test.com', preferredReminderTime: null }
    vi.mocked(api.post).mockResolvedValueOnce({
      data: { accessToken: 'jwt', refreshToken: 'refresh', user: mockUser }
    })
    vi.mocked(api.post).mockResolvedValueOnce({}) // logout call

    const auth = useAuthStore()
    await auth.login('test@test.com', 'password')
    await auth.logout()

    expect(auth.isAuthenticated).toBe(false)
    expect(auth.accessToken).toBeNull()
    expect(localStorage.getItem('accessToken')).toBeNull()
  })

  it('calls register endpoint and authenticates', async () => {
    const mockUser = { id: 'u1', email: 'new@test.com', preferredReminderTime: null }
    vi.mocked(api.post).mockResolvedValueOnce({
      data: { accessToken: 'jwt', refreshToken: 'refresh', user: mockUser }
    })

    const auth = useAuthStore()
    await auth.register('new@test.com', 'password')

    expect(auth.isAuthenticated).toBe(true)
    expect(vi.mocked(api.post)).toHaveBeenCalledWith('/api/auth/register', expect.objectContaining({ email: 'new@test.com' }))
  })
})
