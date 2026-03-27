<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="10" md="7" lg="6">
        <v-card rounded="lg" elevation="4">
          <v-card-title class="text-center pt-6">
            <v-icon size="48" color="primary">mdi-flag-checkered</v-icon>
            <div class="text-h5 mt-2">Choose Your Habit</div>
          </v-card-title>
          <v-card-subtitle class="text-center">Select a habit to start tracking</v-card-subtitle>
          <v-card-text>
            <v-row>
              <v-col v-for="habit in habits.availableHabits" :key="habit.id" cols="12">
                <v-card
                  :variant="selected === habit.id ? 'tonal' : 'outlined'"
                  :color="selected === habit.id ? 'primary' : undefined"
                  @click="selected = habit.id"
                  class="cursor-pointer"
                  rounded="lg"
                >
                  <v-card-title>
                    <v-icon class="mr-2">mdi-food-apple</v-icon>
                    {{ habit.name }}
                  </v-card-title>
                  <v-card-text>{{ habit.description }}</v-card-text>
                </v-card>
              </v-col>
            </v-row>

            <div v-if="selected" class="mt-6">
              <v-text-field
                v-model.number="duration"
                label="Duration (days)"
                type="number"
                :min="7"
                :max="365"
                hint="Research suggests 66 days to form a habit"
                persistent-hint
                prepend-inner-icon="mdi-calendar"
              />
            </div>
          </v-card-text>
          <v-card-actions class="pa-4">
            <v-btn
              block
              color="primary"
              size="large"
              :disabled="!selected"
              :loading="loading"
              @click="enroll"
            >
              Start My {{ duration }}-Day Journey
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useHabitStore } from '@/stores/habit'

const habits = useHabitStore()
const router = useRouter()

const selected = ref<number | null>(null)
const duration = ref(66)
const loading = ref(false)

onMounted(() => habits.loadAvailableHabits())

async function enroll() {
  if (!selected.value) return
  loading.value = true
  try {
    await habits.enroll(selected.value, duration.value)
    router.push({ name: 'dashboard' })
  } finally {
    loading.value = false
  }
}
</script>
