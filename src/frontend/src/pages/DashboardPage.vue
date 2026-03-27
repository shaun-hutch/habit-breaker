<template>
  <AppShell title="Dashboard">
    <v-container>
      <v-row v-if="habits.userHabits.length === 0">
        <v-col>
          <v-empty-state
            icon="mdi-flag-outline"
            title="No habits yet"
            text="Start your journey by setting up a habit to track."
          >
            <template #actions>
              <v-btn color="primary" :to="{ name: 'onboarding' }">Get Started</v-btn>
            </template>
          </v-empty-state>
        </v-col>
      </v-row>

      <v-row v-for="uh in habits.userHabits" :key="uh.id">
        <!-- Progress card -->
        <v-col cols="12" md="6">
          <v-card rounded="lg" elevation="2">
            <v-card-title>
              <v-icon class="mr-2" color="primary">mdi-food-apple</v-icon>
              {{ uh.habit.name }}
            </v-card-title>
            <v-card-text>
              <div class="d-flex align-center justify-space-between mb-2">
                <span class="text-h4 font-weight-bold text-primary">Day {{ uh.dayNumber }}</span>
                <span class="text-body-1 text-medium-emphasis">of {{ uh.totalDays }}</span>
              </div>
              <v-progress-linear
                :model-value="uh.progressPercent"
                color="primary"
                height="12"
                rounded
                class="mb-2"
              />
              <div class="text-body-2 text-medium-emphasis">
                {{ uh.progressPercent }}% complete
                <span v-if="uh.isComplete" class="ml-2 text-success font-weight-bold">🎉 Completed!</span>
              </div>
            </v-card-text>
            <v-card-actions>
              <v-btn color="primary" variant="tonal" :to="{ name: 'log' }">
                <v-icon class="mr-1">mdi-pencil</v-icon> Log Today
              </v-btn>
              <v-btn variant="text" :to="{ name: 'history' }">View History</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>

        <!-- Quick stats -->
        <v-col cols="12" md="6">
          <v-card rounded="lg" elevation="2">
            <v-card-title>
              <v-icon class="mr-2" color="success">mdi-chart-bar</v-icon>
              Stats
            </v-card-title>
            <v-card-text>
              <div class="text-body-1">Started: {{ formatDate(uh.startDate) }}</div>
              <div class="text-body-1 mt-1">
                Target: {{ formatDate(endDate(uh.startDate, uh.totalDays)) }}
              </div>
              <div class="text-body-1 mt-1">Days remaining: {{ Math.max(0, uh.totalDays - uh.dayNumber) }}</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>

      <!-- Push notification settings -->
      <v-row class="mt-4">
        <v-col cols="12" md="6">
          <v-card rounded="lg" elevation="2">
            <v-card-title>
              <v-icon class="mr-2" color="warning">mdi-bell</v-icon>
              Daily Reminder
            </v-card-title>
            <v-card-text>
              <v-text-field
                v-model="reminderTime"
                label="Reminder Time"
                type="time"
                prepend-inner-icon="mdi-clock"
                hint="Set a daily reminder to log your habit"
                persistent-hint
              />
            </v-card-text>
            <v-card-actions>
              <v-btn
                v-if="!push.isSubscribed"
                color="warning"
                variant="tonal"
                @click="enableNotifications"
              >
                Enable Notifications
              </v-btn>
              <template v-else>
                <v-btn color="primary" variant="tonal" @click="saveReminder">Save Reminder</v-btn>
                <v-btn variant="text" @click="push.unsubscribe">Disable</v-btn>
              </template>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </AppShell>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AppShell from '@/components/AppShell.vue'
import { useHabitStore } from '@/stores/habit'
import { usePushStore } from '@/stores/push'
import { useAuthStore } from '@/stores/auth'

const habits = useHabitStore()
const push = usePushStore()
const auth = useAuthStore()

const reminderTime = ref(auth.user?.preferredReminderTime ?? '')

onMounted(() => habits.loadMyHabits())

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
}

function endDate(start: string, days: number) {
  const d = new Date(start)
  d.setDate(d.getDate() + days)
  return d.toISOString().slice(0, 10)
}

async function enableNotifications() {
  await push.subscribe()
  if (reminderTime.value) await push.updateReminderTime(reminderTime.value)
}

async function saveReminder() {
  await push.updateReminderTime(reminderTime.value || null)
}
</script>
