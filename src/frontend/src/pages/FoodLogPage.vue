<template>
  <AppShell title="Log Today">
    <v-container>
      <!-- Offline banner -->
      <v-alert
        v-if="!foodLog.isOnline"
        type="warning"
        variant="tonal"
        icon="mdi-wifi-off"
        class="mb-4"
      >
        You're offline. Your log will be saved locally and synced when you reconnect.
      </v-alert>

      <v-row>
        <v-col cols="12" md="8" offset-md="2">
          <div class="text-h6 mb-4">{{ todayLabel }}</div>

          <v-expansion-panels v-model="openPanels" multiple variant="accordion">
            <v-expansion-panel
              v-for="meal in meals"
              :key="meal.type"
              :value="meal.type"
              rounded="lg"
              class="mb-2"
            >
              <v-expansion-panel-title>
                <v-icon class="mr-2">{{ meal.icon }}</v-icon>
                {{ meal.label }}
                <v-chip
                  v-if="getEntry(meal.type)"
                  class="ml-2"
                  size="small"
                  :color="getEntry(meal.type)?.rating === 0 ? 'success' : 'error'"
                >
                  {{ getEntry(meal.type)?.rating === 0 ? 'Healthy' : 'Unhealthy' }}
                </v-chip>
              </v-expansion-panel-title>
              <v-expansion-panel-text>
                <v-textarea
                  v-model="entries[meal.type].description"
                  :label="`What did you have for ${meal.label}?`"
                  rows="2"
                  auto-grow
                  variant="outlined"
                  class="mb-2"
                />
                <div class="text-body-2 mb-1">How healthy was it?</div>
                <v-btn-toggle
                  v-model="entries[meal.type].rating"
                  mandatory
                  rounded="xl"
                  density="compact"
                >
                  <v-btn :value="0" color="success" variant="tonal">
                    <v-icon class="mr-1">mdi-leaf</v-icon> Healthy
                  </v-btn>
                  <v-btn :value="1" color="error" variant="tonal">
                    <v-icon class="mr-1">mdi-emoticon-sick</v-icon> Unhealthy
                  </v-btn>
                </v-btn-toggle>
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>

          <v-btn
            block
            color="primary"
            size="large"
            :loading="saving"
            class="mt-4"
            @click="save"
          >
            <v-icon class="mr-1">mdi-content-save</v-icon>
            Save Today's Log
          </v-btn>

          <v-snackbar v-model="snack" :color="snackColor" location="bottom">{{ snackMsg }}</v-snackbar>
        </v-col>
      </v-row>
    </v-container>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import AppShell from '@/components/AppShell.vue'
import { useFoodLogStore } from '@/stores/foodLog'
import { useHabitStore } from '@/stores/habit'
import { enqueue } from '@/services/offlineQueue'

const router = useRouter()

const foodLog = useFoodLogStore()
const habits = useHabitStore()

const saving = ref(false)
const snack = ref(false)
const snackMsg = ref('')
const snackColor = ref('success')
const openPanels = ref<number[]>([])

const meals = [
  { type: 0, label: 'Breakfast', icon: 'mdi-weather-sunny' },
  { type: 1, label: 'Morning Tea', icon: 'mdi-coffee' },
  { type: 2, label: 'Lunch', icon: 'mdi-food' },
  { type: 3, label: 'Dinner', icon: 'mdi-silverware-fork-knife' },
  { type: 4, label: 'Supper', icon: 'mdi-moon-waning-crescent' }
]

const entries = reactive<Record<number, { description: string; rating: number }>>({
  0: { description: '', rating: 0 },
  1: { description: '', rating: 0 },
  2: { description: '', rating: 0 },
  3: { description: '', rating: 0 },
  4: { description: '', rating: 0 }
})

const todayLabel = computed(() => {
  return new Date().toLocaleDateString(undefined, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
})

const todayStr = new Date().toISOString().slice(0, 10)
const activeHabit = computed(() => habits.userHabits[0])

function getEntry(type: number) {
  return foodLog.todayLog?.mealEntries.find(e => e.mealType === type)
}

onMounted(async () => {
  await habits.loadMyHabits()
  if (activeHabit.value) {
    await foodLog.loadToday(activeHabit.value.id, todayStr)
    // Pre-populate from existing log
    if (foodLog.todayLog) {
      for (const me of foodLog.todayLog.mealEntries) {
        entries[me.mealType] = { description: me.description, rating: me.rating }
      }
    }
  }
})

async function save() {
  if (!activeHabit.value) return
  saving.value = true
  try {
    const mealEntries = meals
      .filter(m => entries[m.type].description.trim())
      .map(m => ({ mealType: m.type, description: entries[m.type].description, rating: entries[m.type].rating }))

    if (!foodLog.isOnline) {
      await enqueue({ userHabitId: activeHabit.value.id, logDate: todayStr, mealEntries })
      // Register background sync
      if ('serviceWorker' in navigator && 'SyncManager' in window) {
        const reg = await navigator.serviceWorker.ready
        await (reg as ServiceWorkerRegistration & { sync: { register: (tag: string) => Promise<void> } }).sync.register('sync-food-logs')
      }
      snackMsg.value = 'Saved offline — will sync when reconnected.'
      snackColor.value = 'warning'
    } else {
      await foodLog.upsert(activeHabit.value.id, todayStr, mealEntries)
      snackMsg.value = "Today's log saved!"
      snackColor.value = 'success'
      snack.value = true
      setTimeout(() => router.push('/dashboard'), 1200)
      return
    }
    snack.value = true
  } catch {
    snackMsg.value = 'Failed to save. Please try again.'
    snackColor.value = 'error'
    snack.value = true
  } finally {
    saving.value = false
  }
}
</script>
