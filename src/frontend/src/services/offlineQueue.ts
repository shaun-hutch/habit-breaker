import { openDB, type IDBPDatabase } from 'idb'

export interface OfflineEntry {
  id?: number
  userHabitId: number
  logDate: string
  mealEntries: { mealType: number; description: string; rating: number }[]
  syncedAt?: string
}

const DB_NAME = 'habit-breaker'
const STORE_NAME = 'offline-queue'

async function getDb(): Promise<IDBPDatabase> {
  return openDB(DB_NAME, 1, {
    upgrade(db) {
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: 'id', autoIncrement: true })
      }
    }
  })
}

export async function enqueue(entry: OfflineEntry): Promise<void> {
  const db = await getDb()
  // Replace any existing entry for the same date + habit (local wins)
  const all = await db.getAll(STORE_NAME) as OfflineEntry[]
  const existing = all.find(e => e.userHabitId === entry.userHabitId && e.logDate === entry.logDate)
  if (existing?.id !== undefined) {
    await db.delete(STORE_NAME, existing.id)
  }
  await db.add(STORE_NAME, entry)
}

export async function getAll(): Promise<OfflineEntry[]> {
  const db = await getDb()
  return db.getAll(STORE_NAME) as Promise<OfflineEntry[]>
}

export async function remove(id: number): Promise<void> {
  const db = await getDb()
  await db.delete(STORE_NAME, id)
}

export async function clearAll(): Promise<void> {
  const db = await getDb()
  await db.clear(STORE_NAME)
}
