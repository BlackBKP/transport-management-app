const { chromium } = require('C:/Users/Black/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('fs');
const path = require('path');

(async () => {
    const out = path.join(__dirname, 'screenshots');
    fs.mkdirSync(out, { recursive: true });
    const browser = await chromium.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: true });
    const context = await browser.newContext({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1 });
    await context.addInitScript(() => localStorage.setItem('tms-theme', 'light'));
    const page = await context.newPage();
    const results = [];
    for (const [route, name] of [['Index', '01-dashboard'], ['Create', '02-book-transport'], ['Bookings', '03-bookings'], ['Vehicles', '04-fleet'], ['Routes', '05-routes'], ['Shifts', '06-shifts'], ['Map', '07-route-map']]) {
        const response = await page.goto(`http://127.0.0.1:5090/Transport/${route}`, { waitUntil: 'networkidle' });
        if (!response.ok()) throw new Error(`${route}: ${response.status()}`);
        await page.evaluate(() => document.fonts.ready);
        if (route === 'Create') {
            await page.locator('#employeeId').fill('EMP0031');
            await page.locator('#employeeName').fill('Alex Morgan');
            await page.locator('#department').fill('Operations');
            await page.locator('#shiftId').selectOption({ index: 1 });
            await page.locator('#routeId').selectOption({ index: 1 });
            await page.locator('#pickupPoint').selectOption({ index: 1 });
            await page.locator('#vehicleId').selectOption({ index: 1 });
        }
        if (route === 'Map') await page.locator('.map-route').first().click();
        await page.screenshot({ path: path.join(out, `${name}.png`), fullPage: false });
        results.push({ file: `${name}.png`, title: await page.title(), width: 1600 });
        console.log(`Captured ${name}`);
    }
    await page.goto('http://127.0.0.1:5090/Transport/Index', { waitUntil: 'networkidle' });
    await page.locator('#theme-toggle').click();
    await page.screenshot({ path: path.join(out, '08-dashboard-dark.png'), fullPage: false });
    await page.locator('#theme-toggle').click();
    await page.setViewportSize({ width: 390, height: 844 });
    await page.screenshot({ path: path.join(out, '09-dashboard-mobile.png'), fullPage: true });
    fs.writeFileSync(path.join(out, 'manifest.json'), JSON.stringify(results, null, 2));
    await browser.close();
})().catch(error => { console.error(error); process.exit(1); });



