import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useFoodLogStore } from '@/stores/foodLog'

vi.mock('@/services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() }
    }
  }
}))

import api from '@/services/api'

describe('useFoodLogStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('loads today log successfully', async () => {
    const mockLog = {
      id: 1,
      userHabitId: 10,
      logDate: '2026-01-01',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      mealEntries: []
    }
    vi.mocked(api.get).mockResolvedValueOnce({ data: mockLog })

    const store = useFoodLogStore()
    await store.loadToday(10, '2026-01-01')

    expect(store.todayLog?.id).toBe(1)
    expect(store.todayLog?.logDate).toBe('2026-01-01')
  })

  it('sets todayLog to null when 404', async () => {
    const error = Object.assign(new Error(), { response: { status: 404 } })
    vi.mocked(api.get).mockRejectedValueOnce(error)

    const store = useFoodLogStore()
    await store.loadToday(10, '2026-01-01')

    expect(store.todayLog).toBeNull()
  })

  it('upsert returns saved log and updates todayLog', async () => {
    const savedLog = {
      id: 2,
      userHabitId: 10,
      logDate: '2026-01-01',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      mealEntries: [{ id: 1, mealType: 0, mealTypeName: 'Breakfast', description: 'Eggs', rating: 0, ratingName: 'Healthy' }]
    }
    vi.mocked(api.post).mockResolvedValueOnce({ data: savedLog })

    const store = useFoodLogStore()
    const result = await store.upsert(10, '2026-01-01', [{ mealType: 0, description: 'Eggs', rating: 0 }])

    expect(store.todayLog?.id).toBe(2)
    expect(result.mealEntries).toHaveLength(1)
  })

  it('loads history with pagination', async () => {
    const mockHistory = {
      items: [{ id: 1, userHabitId: 10, logDate: '2026-01-01', createdAt: '', updatedAt: '', mealEntries: [] }],
      totalCount: 1,
      page: 1,
      pageSize: 20
    }
    vi.mocked(api.get).mockResolvedValueOnce({ data: mockHistory })

    const store = useFoodLogStore()
    await store.loadHistory(10, 1, 20)

    expect(store.history).toHaveLength(1)
    expect(store.historyTotal).toBe(1)
  })
})
