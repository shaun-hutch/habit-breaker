const fs = require('fs')
const path = require('path')

const iconsDir = path.join(__dirname, 'public', 'icons')
fs.mkdirSync(iconsDir, { recursive: true })

// Minimal valid 1x1 transparent PNG – placeholder until real icons are designed
// This is a valid PNG that browsers will accept for the PWA manifest
const minimalPng = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==',
  'base64'
)

fs.writeFileSync(path.join(iconsDir, 'icon-192x192.png'), minimalPng)
fs.writeFileSync(path.join(iconsDir, 'icon-512x512.png'), minimalPng)
console.log('Placeholder icons created at public/icons/')
