const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const http = require('node:http');
const path = require('node:path');
const { chromium } = require('playwright');

const root = path.resolve(process.argv[2] || 'release/wwwroot');
const types = { '.html': 'text/html', '.js': 'text/javascript', '.json': 'application/json',
    '.css': 'text/css', '.wasm': 'application/wasm', '.png': 'image/png', '.ttf': 'font/ttf' };

async function main() {
    const server = http.createServer(async (request, response) => {
        const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
        const filename = path.resolve(root, '.' + (pathname === '/' ? '/index.html' : pathname));
        if (!filename.startsWith(root + path.sep)) {
            response.writeHead(403).end();
            return;
        }
        try {
            const bytes = await fs.readFile(filename);
            response.writeHead(200, { 'Content-Type': types[path.extname(filename)] || 'application/octet-stream' });
            response.end(bytes);
        } catch {
            response.writeHead(404).end();
        }
    });
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    let browser;
    const errors = [];
    try {
        browser = await chromium.launch({ headless: true, channel: process.env.PLAYWRIGHT_CHANNEL || undefined });
        const page = await browser.newPage();
        page.on('pageerror', error => errors.push(error.message));
        page.on('console', message => {
            if (message.type() === 'error') errors.push(message.text());
        });
        await page.goto(`http://127.0.0.1:${server.address().port}`, { waitUntil: 'domcontentloaded' });
        const input = page.getByPlaceholder('Enter some text');
        await input.waitFor({ timeout: 30000 });
        await input.fill('waiting for dotnet 11');
        await page.getByRole('button', { name: 'Generate', exact: true }).click();
        await page.waitForFunction(() => {
            const image = document.querySelector('img.generated-image');
            return image?.complete && image.naturalWidth > 0;
        }, null, { timeout: 30000 });
        const image = await page.locator('img.generated-image').evaluate(element => {
            window.smokeFirstImage = element.src;
            return { width: element.naturalWidth, height: element.naturalHeight, source: element.src };
        });
        assert.equal(image.width, 800);
        assert.equal(image.height, 600);
        assert.ok(image.source.startsWith('data:image/png;base64,'));
        assert.deepEqual([...Buffer.from(image.source.split(',')[1], 'base64').subarray(0, 8)],
            [137, 80, 78, 71, 13, 10, 26, 10]);

        await input.fill('testing again');
        await page.getByRole('button', { name: 'Generate', exact: true }).click();
        await page.waitForFunction(() => {
            const image = document.querySelector('img.generated-image');
            return image?.complete && image.naturalWidth > 0 && image.src !== window.smokeFirstImage;
        }, null, { timeout: 30000 });
        assert.deepEqual(errors, []);
        console.log('Release AOT browser smoke test passed: generated two distinct 800x600 PNG images.');
    } catch (error) {
        if (errors.length) console.error('Browser errors:', errors.join('\n'));
        throw error;
    } finally {
        if (browser) await browser.close();
        server.closeAllConnections();
        await new Promise(resolve => server.close(resolve));
    }
}

main().catch(error => { console.error(error); process.exitCode = 1; });
