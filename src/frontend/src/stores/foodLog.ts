import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'

export interface MealEntry {
  id?: number
  mealType: number
  mealTypeName: string
  description: string
  rating: number
  ratingName: string
}

export interface FoodLog {
  id: number
  userHabitId: number
  logDate: string
  createdAt: string
  updatedAt: string
  mealEntries: MealEntry[]
}

export const useFoodLogStore = defineStore('foodLog', () => {
  const todayLog = ref<FoodLog | null>(null)
  const history = ref<FoodLog[]>([])
  const historyTotal = ref(0)
  const isOnline = ref(navigator.onLine)

  window.addEventListener('online', () => { isOnline.value = true })
  window.addEventListener('offline', () => { isOnline.value = false })

  async function loadToday(userHabitId: number, date: string) {
    try {
      const res = await api.get(`/api/food-logs?userHabitId=${userHabitId}&date=${date}`)
      todayLog.value = res.data
    } catch (e: unknown) {
      const err = e as { response?: { status?: number } }
      if (err.response?.status === 404) todayLog.value = null
      else throw e
    }
  }

  async function upsert(userHabitId: number, logDate: string, mealEntries: { mealType: number; description: string; rating: number }[]) {
    const res = await api.post('/api/food-logs', { userHabitId, logDate, mealEntries })
    todayLog.value = res.data
    return res.data as FoodLog
  }

  async function loadHistory(userHabitId: number, page = 1, pageSize = 20) {
    const res = await api.get(`/api/food-logs/history?userHabitId=${userHabitId}&page=${page}&pageSize=${pageSize}`)
    history.value = res.data.items
    historyTotal.value = res.data.totalCount
  }

  async function deleteLog(id: number) {
    await api.delete(`/api/food-logs/${id}`)
    if (todayLog.value?.id === id) todayLog.value = null
  }

  return { todayLog, history, historyTotal, isOnline, loadToday, upsert, loadHistory, deleteLog }
})
