<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="5" lg="4">
        <v-card rounded="lg" elevation="4">
          <v-card-title class="text-center pt-6 pb-2">
            <v-icon size="48" color="primary">mdi-leaf</v-icon>
            <div class="text-h5 mt-2">Habit Breaker</div>
          </v-card-title>
          <v-card-subtitle class="text-center">Sign in to continue</v-card-subtitle>
          <v-card-text class="pt-4">
            <v-alert v-if="error" type="error" variant="tonal" class="mb-4">{{ error }}</v-alert>
            <v-form ref="form" @submit.prevent="submit" validate-on="submit">
              <v-text-field
                v-model="email"
                label="Email"
                type="email"
                prepend-inner-icon="mdi-email"
                :rules="[required, emailRule]"
                autocomplete="email"
                class="mb-2"
              />
              <v-text-field
                v-model="password"
                label="Password"
                :type="showPass ? 'text' : 'password'"
                prepend-inner-icon="mdi-lock"
                :append-inner-icon="showPass ? 'mdi-eye-off' : 'mdi-eye'"
                @click:append-inner="showPass = !showPass"
                :rules="[required]"
                autocomplete="current-password"
              />
              <v-btn
                block
                color="primary"
                size="large"
                type="submit"
                :loading="loading"
                class="mt-4"
              >Sign In</v-btn>
            </v-form>
          </v-card-text>
          <v-card-actions class="justify-center pb-6">
            <span class="text-body-2 text-medium-emphasis">Don't have an account?</span>
            <v-btn variant="text" color="primary" :to="{ name: 'register' }" class="ml-1">Register</v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useHabitStore } from '@/stores/habit'

const auth = useAuthStore()
const habits = useHabitStore()
const router = useRouter()

const form = ref()
const email = ref('')
const password = ref('')
const showPass = ref(false)
const loading = ref(false)
const error = ref('')

const required = (v: string) => !!v || 'Required'
const emailRule = (v: string) => /.+@.+\..+/.test(v) || 'Valid email required'

async function submit() {
  const { valid } = await form.value.validate()
  if (!valid) return
  loading.value = true
  error.value = ''
  try {
    await auth.login(email.value, password.value)
    await habits.loadMyHabits()
    if (habits.userHabits.length === 0) {
      router.push({ name: 'onboarding' })
    } else {
      router.push({ name: 'dashboard' })
    }
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error ?? 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>
