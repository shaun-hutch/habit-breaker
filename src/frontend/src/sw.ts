/// <reference lib="webworker" />
import { getAll, remove } from './services/offlineQueue'
import api from './services/api'

declare const self: ServiceWorkerGlobalScope

// ─── Push notification handler ───────────────────────────────────────────────
self.addEventListener('push', (event: PushEvent) => {
  if (!event.data) return
  let data: { title?: string; body?: string; icon?: string } = {}
  try {
    data = event.data.json()
  } catch {
    data = { title: 'Habit Reminder', body: event.data.text() }
  }
  event.waitUntil(
    self.registration.showNotification(data.title ?? 'Habit Reminder', {
      body: data.body ?? "Don't forget to log your habits today!",
      icon: data.icon ?? '/icons/icon-192x192.png',
      badge: '/icons/icon-192x192.png',
      tag: 'habit-reminder',
      renotify: true
    } as NotificationOptions)
  )
})

// ─── Notification click handler ──────────────────────────────────────────────
self.addEventListener('notificationclick', (event: NotificationEvent) => {
  event.notification.close()
  event.waitUntil(
    self.clients.matchAll({ type: 'window' }).then(clients => {
      const existing = clients.find(c => c.url.includes('/log'))
      if (existing) return existing.focus()
      return self.clients.openWindow('/log')
    })
  )
})

// ─── Background Sync handler ─────────────────────────────────────────────────
self.addEventListener('sync', (event: Event) => {
  const syncEvent = event as Event & { tag: string; waitUntil(p: Promise<any>): void }
  if (syncEvent.tag === 'sync-food-logs') {
    syncEvent.waitUntil(syncOfflineQueue())
  }
})

async function syncOfflineQueue() {
  const entries = await getAll()
  for (const entry of entries) {
    try {
      await api.post('/api/food-logs', {
        userHabitId: entry.userHabitId,
        logDate: entry.logDate,
        mealEntries: entry.mealEntries
      })
      if (entry.id !== undefined) {
        await remove(entry.id)
      }
    } catch {
      // Will retry on next sync event
    }
  }
}
