import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'

export const usePushStore = defineStore('push', () => {
  const isSubscribed = ref(false)
  const reminderTime = ref<string | null>(null)

  async function subscribe() {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return
    const permission = await Notification.requestPermission()
    if (permission !== 'granted') return

    const reg = await navigator.serviceWorker.ready
    const vapidKey = import.meta.env.VITE_VAPID_PUBLIC_KEY as string
    const subscription = await reg.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(vapidKey)
    })
    const sub = subscription.toJSON()
    await api.post('/api/push/subscribe', {
      endpoint: sub.endpoint,
      p256dh: sub.keys?.p256dh,
      auth: sub.keys?.auth
    })
    isSubscribed.value = true
  }

  async function unsubscribe() {
    const reg = await navigator.serviceWorker.ready
    const subscription = await reg.pushManager.getSubscription()
    if (subscription) {
      await api.delete('/api/push/subscribe', { data: { endpoint: subscription.endpoint } })
      await subscription.unsubscribe()
    }
    isSubscribed.value = false
  }

  async function updateReminderTime(time: string | null) {
    await api.put('/api/push/reminder-time', { reminderTime: time })
    reminderTime.value = time
  }

  function urlBase64ToUint8Array(base64String: string): Uint8Array {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4)
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/')
    const rawData = window.atob(base64)
    return Uint8Array.from([...rawData].map(char => char.charCodeAt(0)))
  }

  return { isSubscribed, reminderTime, subscribe, unsubscribe, updateReminderTime }
})
