<template>
  <v-app>
    <v-navigation-drawer v-model="drawer" temporary>
      <v-list-item
        prepend-icon="mdi-leaf"
        title="Habit Breaker"
        :subtitle="auth.user?.email"
        nav
      />
      <v-divider />
      <v-list density="compact" nav>
        <v-list-item prepend-icon="mdi-view-dashboard" title="Dashboard" :to="{ name: 'dashboard' }" />
        <v-list-item prepend-icon="mdi-pencil" title="Log Today" :to="{ name: 'log' }" />
        <v-list-item prepend-icon="mdi-history" title="History" :to="{ name: 'history' }" />
      </v-list>
      <template #append>
        <v-divider />
        <v-list density="compact" nav>
          <v-list-item prepend-icon="mdi-logout" title="Sign Out" @click="handleLogout" />
        </v-list>
      </template>
    </v-navigation-drawer>

    <v-app-bar color="primary" elevation="2">
      <v-app-bar-nav-icon @click="drawer = !drawer" />
      <v-app-bar-title>{{ title }}</v-app-bar-title>
    </v-app-bar>

    <v-main>
      <slot />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

defineProps<{ title?: string }>()

const auth = useAuthStore()
const router = useRouter()
const drawer = ref(false)

async function handleLogout() {
  await auth.logout()
  router.push({ name: 'login' })
}
</script>
