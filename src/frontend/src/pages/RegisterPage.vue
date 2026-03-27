<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="5" lg="4">
        <v-card rounded="lg" elevation="4">
          <v-card-title class="text-center pt-6 pb-2">
            <v-icon size="48" color="primary">mdi-leaf</v-icon>
            <div class="text-h5 mt-2">Create Account</div>
          </v-card-title>
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
                :rules="[required, passwordRule]"
                autocomplete="new-password"
                class="mb-2"
              />
              <v-text-field
                v-model="confirmPassword"
                label="Confirm Password"
                :type="showPass ? 'text' : 'password'"
                prepend-inner-icon="mdi-lock-check"
                :rules="[required, matchRule]"
                autocomplete="new-password"
              />
              <v-btn block color="primary" size="large" type="submit" :loading="loading" class="mt-4">Register</v-btn>
            </v-form>
          </v-card-text>
          <v-card-actions class="justify-center pb-6">
            <span class="text-body-2 text-medium-emphasis">Already have an account?</span>
            <v-btn variant="text" color="primary" :to="{ name: 'login' }" class="ml-1">Sign In</v-btn>
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

const auth = useAuthStore()
const router = useRouter()

const form = ref()
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPass = ref(false)
const loading = ref(false)
const error = ref('')

const required = (v: string) => !!v || 'Required'
const emailRule = (v: string) => /.+@.+\..+/.test(v) || 'Valid email required'
const passwordRule = (v: string) => (v.length >= 8 && /[A-Z]/.test(v) && /[^a-zA-Z0-9]/.test(v)) || 'Min 8 chars, one uppercase, one special character'
const matchRule = (v: string) => v === password.value || 'Passwords do not match'

async function submit() {
  const { valid } = await form.value.validate()
  if (!valid) return
  loading.value = true
  error.value = ''
  try {
    await auth.register(email.value, password.value)
    router.push({ name: 'onboarding' })
  } catch (e: unknown) {
    const err = e as { response?: { data?: { errors?: string[] } } }
    error.value = err.response?.data?.errors?.join(', ') ?? 'Registration failed.'
  } finally {
    loading.value = false
  }
}
</script>
