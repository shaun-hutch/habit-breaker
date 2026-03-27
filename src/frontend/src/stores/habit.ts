import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'

export interface Habit {
  id: number
  name: string
  description: string
  defaultDurationDays: number
}

export interface UserHabit {
  id: number
  habit: Habit
  startDate: string
  durationDays: number
  dayNumber: number
  totalDays: number
  progressPercent: number
  isComplete: boolean
}

export const useHabitStore = defineStore('habit', () => {
  const availableHabits = ref<Habit[]>([])
  const userHabits = ref<UserHabit[]>([])

  async function loadAvailableHabits() {
    const res = await api.get('/api/habits')
    availableHabits.value = res.data
  }

  async function loadMyHabits() {
    const res = await api.get('/api/habits/mine')
    userHabits.value = res.data
  }

  async function enroll(habitId: number, durationDays?: number) {
    const res = await api.post('/api/habits/enroll', { habitId, durationDays })
    userHabits.value.push(res.data)
    return res.data as UserHabit
  }

  return { availableHabits, userHabits, loadAvailableHabits, loadMyHabits, enroll }
})
