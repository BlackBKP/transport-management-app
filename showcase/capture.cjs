const { chromium } = require('C:/Users/Black/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs = require('fs');
const path = require('path');
const out = __dirname;
(async () => {
 const browser = await chromium.launch({executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,args:['--autoplay-policy=no-user-gesture-required']});
 const page = await browser.newPage({viewport:{width:1440,height:740},deviceScaleFactor:1});
 await page.route('**/css/**',async route=>route.fulfill({path:path.join(__dirname,'../TMS/wwwroot/css/site.css'),contentType:'text/css'}));
 await page.route('**/js/**',async route=>{const name=new URL(route.request().url()).pathname.split('/').pop().replace(/\.[a-z0-9]{10}(?=\.js)/,'');await route.fulfill({path:path.join(__dirname,'../TMS/wwwroot/js',name),contentType:'application/javascript'});});
 const scenes = [
 ['Index','Transport, brought together.','TMS  /  TRANSPORTATION MANAGEMENT SYSTEM',8],
 ['Index','See the day at a glance.','DASHBOARD  /  BOOKINGS, SHIFTS & VEHICLE TOTALS',8],
 ['Create','Plan the next journey.','RESERVATIONS  /  EMPLOYEE, ROUTE, PICKUP & SHIFT',9],
 ['Bookings','Keep every booking in view.','EMPLOYEE BOOKINGS  /  SEARCH & FILTER RESERVATIONS',8],
 ['Vehicles','Know your fleet.','FLEET  /  VEHICLE DETAILS & SEAT CAPACITY',7],
 ['Routes','Connect people and pickup points.','ROUTES  /  EXPLORE THE TRANSPORT NETWORK',7],
 ['Shifts','Keep teams moving on schedule.','SHIFTS  /  DEPARTURE & WORKING TIMES',6],
 ['Index','One workspace. Every journey.','TMS  /  YOUR DAILY TRANSPORT OVERVIEW',7]
 ];
 for(let i=0;i<scenes.length;i++){
  await page.goto('http://127.0.0.1:5088/Transport/'+scenes[i][0],{waitUntil:'networkidle'});
  await page.evaluate(()=>{localStorage.setItem('tms-theme','light');document.documentElement.setAttribute('data-theme','light');});
  if(scenes[i][0]==='Create'){
   await page.locator('#employeeId').fill('EMP0031');
   await page.locator('#employeeName').fill('Alex Morgan');
   await page.locator('#department').fill('Operations');
   await page.locator('#shiftId').selectOption({index:1});
   await page.locator('#routeId').selectOption({index:1});
  }
  await page.screenshot({path:path.join(out,`screen-${i}.png`)});
  console.log('Captured',scenes[i][0]);
 }
 fs.writeFileSync(path.join(out,'scenes.json'),JSON.stringify(scenes));
 await browser.close();
})();

