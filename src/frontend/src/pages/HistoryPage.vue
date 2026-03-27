<template>
  <AppShell title="History">
    <v-container>
      <v-row>
        <v-col cols="12" md="8" offset-md="2">
          <v-card rounded="lg" elevation="2">
            <v-card-title>Food Log History</v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field v-model="from" label="From" type="date" density="compact" />
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field v-model="to" label="To" type="date" density="compact" />
                </v-col>
              </v-row>
              <v-btn color="primary" variant="tonal" @click="load" :loading="loading">
                <v-icon class="mr-1">mdi-magnify</v-icon> Search
              </v-btn>
            </v-card-text>
          </v-card>

          <div v-if="foodLog.history.length === 0 && !loading" class="text-center mt-8 text-medium-emphasis">
            <v-icon size="64" class="mb-2">mdi-calendar-blank</v-icon>
            <div>No logs found for this period.</div>
          </div>

          <v-card
            v-for="log in foodLog.history"
            :key="log.id"
            rounded="lg"
            elevation="2"
            class="mt-4"
          >
            <v-card-title>
              <v-icon class="mr-2" color="primary">mdi-calendar</v-icon>
              {{ formatDate(log.logDate) }}
            </v-card-title>
            <v-card-text>
              <div v-for="entry in log.mealEntries" :key="entry.id" class="mb-2">
                <div class="d-flex align-center">
                  <v-chip
                    size="small"
                    :color="entry.rating === 0 ? 'success' : 'error'"
                    class="mr-2"
                  >
                    {{ entry.ratingName }}
                  </v-chip>
                  <span class="font-weight-medium">{{ entry.mealTypeName }}:</span>
                  <span class="ml-1 text-body-2">{{ entry.description }}</span>
                </div>
              </div>
              <div v-if="log.mealEntries.length === 0" class="text-medium-emphasis">No meals logged.</div>
            </v-card-text>
          </v-card>

          <!-- Pagination -->
          <div class="d-flex justify-center mt-4" v-if="foodLog.historyTotal > pageSize">
            <v-pagination
              v-model="page"
              :length="Math.ceil(foodLog.historyTotal / pageSize)"
              @update:model-value="load"
            />
          </div>
        </v-col>
      </v-row>
    </v-container>
  </AppShell>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AppShell from '@/components/AppShell.vue'
import { useFoodLogStore } from '@/stores/foodLog'
import { useHabitStore } from '@/stores/habit'

const foodLog = useFoodLogStore()
const habits = useHabitStore()

const loading = ref(false)
const page = ref(1)
const pageSize = 20

const thirtyDaysAgo = new Date()
thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30)
const from = ref(thirtyDaysAgo.toISOString().slice(0, 10))
const to = ref(new Date().toISOString().slice(0, 10))

const activeHabit = computed(() => habits.userHabits[0])

function formatDate(dateStr: string) {
  return new Date(dateStr + 'T00:00:00').toLocaleDateString(undefined, {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
  })
}

async function load() {
  if (!activeHabit.value) return
  loading.value = true
  try {
    await foodLog.loadHistory(activeHabit.value.id, page.value, pageSize)
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await habits.loadMyHabits()
  await load()
})
</script>

<script lang="ts">
import { computed } from 'vue'
export default { name: 'HistoryPage' }
</script>
