import { test, expect } from '@playwright/test'

const email = `onboard-${Date.now()}@test.com`
const password = 'Test!Password1'

test.describe('Onboarding', () => {
  test.beforeEach(async ({ page }) => {
    // Register fresh user
    await page.goto('/register')
    await page.fill('input[type="email"]', email)
    const pwFields = page.locator('input[type="password"]')
    await pwFields.nth(0).fill(password)
    await pwFields.nth(1).fill(password)
    await page.click('button[type="submit"]')
    await page.waitForURL(/\/onboarding/)
  })

  test('shows "Better Eating" habit card', async ({ page }) => {
    await expect(page.locator('.v-card', { hasText: 'Better Eating' })).toBeVisible()
  })

  test('select habit and start journey', async ({ page }) => {
    await page.locator('.v-card.cursor-pointer', { hasText: 'Better Eating' }).click()
    await page.click('button:has-text("Journey")')
    await expect(page).toHaveURL(/\/dashboard/)
    await expect(page.locator('text=Day 1')).toBeVisible()
  })

  test('custom duration is shown in button', async ({ page }) => {
    await page.locator('.v-card.cursor-pointer', { hasText: 'Better Eating' }).click()
    const durationField = page.locator('input[type="number"]')
    await durationField.clear()
    await durationField.fill('30')
    await expect(page.locator('button:has-text("30-Day")')).toBeVisible()
  })
})
