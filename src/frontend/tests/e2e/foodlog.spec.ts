import { test, expect } from '@playwright/test'

const email = `foodlog-${Date.now()}@test.com`
const password = 'Test!Password1'

test.describe('Food Log', () => {
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
    await page.goto('/log')
  })

  test('shows all 5 meal sections', async ({ page }) => {
    for (const meal of ['Breakfast', 'Morning Tea', 'Lunch', 'Dinner', 'Supper']) {
      await expect(page.locator('.v-expansion-panel-title', { hasText: meal })).toBeVisible()
    }
  })

  test('log a meal and save', async ({ page }) => {
    // Expand Breakfast
    await page.locator('.v-expansion-panel-title', { hasText: 'Breakfast' }).click()
    await page.locator('textarea').first().fill('Eggs and toast')
    // Select Healthy
    await page.locator('button:has-text("Healthy")').first().click()
    // Save
    await page.click('button:has-text("Save Today")')
    await expect(page.locator('.v-snackbar')).toBeVisible()
    await expect(page.locator('.v-snackbar')).toContainText("saved")
  })

  test('healthy/unhealthy toggle changes chip', async ({ page }) => {
    await page.locator('.v-expansion-panel-title', { hasText: 'Lunch' }).click()
    await page.locator('textarea').nth(2).fill('Burger')
    await page.locator('button:has-text("Unhealthy")').nth(2).click()
    await page.click('button:has-text("Save Today")')
    // After save the chip should show Unhealthy
    await expect(page.locator('.v-chip', { hasText: 'Unhealthy' })).toBeVisible()
  })
})
