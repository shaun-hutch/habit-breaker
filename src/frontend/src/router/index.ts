import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('@/pages/LoginPage.vue'), meta: { requiresAuth: false } },
    { path: '/register', name: 'register', component: () => import('@/pages/RegisterPage.vue'), meta: { requiresAuth: false } },
    { path: '/onboarding', name: 'onboarding', component: () => import('@/pages/OnboardingPage.vue'), meta: { requiresAuth: true } },
    { path: '/dashboard', name: 'dashboard', component: () => import('@/pages/DashboardPage.vue'), meta: { requiresAuth: true } },
    { path: '/log', name: 'log', component: () => import('@/pages/FoodLogPage.vue'), meta: { requiresAuth: true } },
    { path: '/history', name: 'history', component: () => import('@/pages/HistoryPage.vue'), meta: { requiresAuth: true } },
    { path: '/', redirect: '/dashboard' },
    { path: '/:pathMatch(.*)*', redirect: '/dashboard' }
  ]
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }
  if (!to.meta.requiresAuth && auth.isAuthenticated && (to.name === 'login' || to.name === 'register')) {
    return { name: 'dashboard' }
  }
})

export default router
