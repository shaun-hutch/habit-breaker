import { test, expect } from '@playwright/test'

const email = `history-${Date.now()}@test.com`
const password = 'Test!Password1'

test.describe('History', () => {
  test.beforeEach(async ({ page }) => {
    // Register and complete onboarding
    await page.goto('/register')
    await page.fill('input[type="email"]', email)
    const pwFields = page.locator('input[type="password"]')
    await pwFields.nth(0).fill(password)
    await pwFields.nth(1).fill(password)
    await page.click('button[type="submit"]')
    await page.waitForURL(/\/onboarding/)
    await page.locator('.v-card.cursor-pointer').click()
    await page.click('button:has-text("Journey")')
    await page.waitForURL(/\/dashboard/)

    // Log a meal for today
    await page.goto('/log')
    await page.locator('.v-expansion-panel-title', { hasText: 'Breakfast' }).click()
    await page.locator('textarea').first().fill('Oats')
    await page.locator('button:has-text("Healthy")').first().click()
    await page.click('button:has-text("Save Today")')
    await page.locator('.v-snackbar').waitFor()
  })

  test('history page is accessible from nav', async ({ page }) => {
    await page.goto('/history')
    await expect(page).toHaveURL(/\/history/)
    await expect(page.locator('h1, .text-h5, .text-h4', { hasText: /history/i })).toBeVisible()
  })

  test('shows logged entry for today', async ({ page }) => {
    await page.goto('/history')
    const today = new Date().toISOString().split('T')[0]
    // The history card for today should be visible
    await expect(page.locator('.v-card').filter({ hasText: today })).toBeVisible()
  })

  test('date filter limits results', async ({ page }) => {
    await page.goto('/history')
    const yesterday = new Date(Date.now() - 86400000).toISOString().split('T')[0]
    // Set "from" date to yesterday and "to" date to yesterday — no results for today
    const dateInputs = page.locator('input[type="date"]')
    await dateInputs.nth(0).fill(yesterday)
    await dateInputs.nth(1).fill(yesterday)
    await page.click('button:has-text("Search")')
    const today = new Date().toISOString().split('T')[0]
    await expect(page.locator('.v-card').filter({ hasText: today })).not.toBeVisible()
  })
})
