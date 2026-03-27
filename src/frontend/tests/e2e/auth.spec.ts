import { test, expect } from '@playwright/test'

const testEmail = `pw-test-${Date.now()}@test.com`
const testPassword = 'Test!Password1'

test.describe('Authentication', () => {
  test('register new user navigates to onboarding', async ({ page }) => {
    await page.goto('/register')
    await page.fill('input[type="email"]', testEmail)
    await page.fill('input[type="password"]:first-of-type', testPassword)
    await page.fill('input[type="password"]:last-of-type', testPassword)
    await page.click('button[type="submit"]')
    await expect(page).toHaveURL(/\/onboarding/)
  })

  test('login with valid credentials navigates to dashboard', async ({ page }) => {
    // Register first
    await page.goto('/register')
    await page.fill('input[type="email"]', testEmail)
    const pwFields = page.locator('input[type="password"]')
    await pwFields.nth(0).fill(testPassword)
    await pwFields.nth(1).fill(testPassword)
    await page.click('button[type="submit"]')

    // Complete onboarding
    await page.waitForURL(/\/onboarding/)
    await page.click('.v-card.cursor-pointer')
    await page.click('button:has-text("Journey")')

    // Logout
    await page.goto('/login')
    await page.fill('input[type="email"]', testEmail)
    await page.fill('input[type="password"]', testPassword)
    await page.click('button[type="submit"]')
    await expect(page).toHaveURL(/\/dashboard/)
  })

  test('login with wrong password shows error', async ({ page }) => {
    await page.goto('/login')
    await page.fill('input[type="email"]', 'bad@bad.com')
    await page.fill('input[type="password"]', 'WrongPass!1')
    await page.click('button[type="submit"]')
    await expect(page.locator('.v-alert')).toBeVisible()
  })

  test('unauthenticated access redirects to login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})
