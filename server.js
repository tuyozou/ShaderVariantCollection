const express = require('express');
const fs = require('fs');
const path = require('path');

const app = express();
const PORT = 8880;
const DATA_FILE = path.join(__dirname, 'data.json');

// Middleware
app.use(express.json({ limit: '10mb' }));
app.use(express.static(path.join(__dirname, 'public')));

// In-memory data and deduplication map
let dataList = [];
let messageMap = new Map(); // message -> { index, versions: Set }

// Semver comparison function with fallback to string sort
function compareSemver(a, b) {
    const parseVersion = (v) => {
        const match = v.match(/^(\d+)(?:\.(\d+))?(?:\.(\d+))?/);
        if (match) {
            return [
                parseInt(match[1], 10) || 0,
                parseInt(match[2], 10) || 0,
                parseInt(match[3], 10) || 0
            ];
        }
        return null;
    };
    
    const aParts = parseVersion(a);
    const bParts = parseVersion(b);
    
    // If both are valid semver, compare numerically
    if (aParts && bParts) {
        for (let i = 0; i < 3; i++) {
            if (aParts[i] !== bParts[i]) {
                return aParts[i] - bParts[i];
            }
        }
        return 0;
    }
    
    // Fallback to string comparison
    return a.localeCompare(b);
}

// Sort versions array using semver comparison
function sortVersions(versions) {
    return [...versions].sort(compareSemver);
}

// Load data from JSON file on startup
function loadData() {
    try {
        if (fs.existsSync(DATA_FILE)) {
            const content = fs.readFileSync(DATA_FILE, 'utf-8');
            dataList = JSON.parse(content);
            // Rebuild the message -> versions map
            messageMap = new Map();
            dataList.forEach((item, index) => {
                const versions = new Set(item.versions || []);
                messageMap.set(item.message, { index, versions });
            });
            console.log(`Loaded ${dataList.length} records from data.json`);
        }
    } catch (err) {
        console.error('Error loading data:', err);
        dataList = [];
        messageMap = new Map();
    }
}

// Save data to JSON file
function saveData() {
    try {
        fs.writeFileSync(DATA_FILE, JSON.stringify(dataList, null, 2), 'utf-8');
    } catch (err) {
        console.error('Error saving data:', err);
    }
}

// POST /submit - Submit variant data
app.post('/submit', (req, res) => {
    const { message, version } = req.body;
    
    if (!message) {
        return res.status(400).json({ ok: false, error: 'message is required' });
    }
    
    const ver = version || 'unknown';
    
    // Check if message exists
    if (messageMap.has(message)) {
        const entry = messageMap.get(message);
        
        // Check if version already exists
        if (entry.versions.has(ver)) {
            return res.json({ ok: false, exists: true });
        }
        
        // Add new version to existing message
        entry.versions.add(ver);
        dataList[entry.index].versions = sortVersions(entry.versions);
        saveData();
        
        return res.json({ ok: true });
    }
    
    // Add new entry
    const newEntry = {
        message: message,
        time: new Date().toISOString(),
        versions: [ver]
    };
    
    const index = dataList.length;
    dataList.push(newEntry);
    messageMap.set(message, { index, versions: new Set([ver]) });
    saveData();
    
    res.json({ ok: true });
});

// GET /data - Get all variant data (reverse chronological order)
app.get('/data', (req, res) => {
    // Return data in reverse order (newest first)
    const reversed = [...dataList].reverse();
    res.json(reversed);
});

// POST /clear - Clear all data
app.post('/clear', (req, res) => {
    dataList = [];
    messageMap = new Map();
    saveData();
    res.json({ ok: true });
});

// Load data and start server
loadData();
app.listen(PORT, '0.0.0.0', () => {
    const os = require('os');
    const interfaces = os.networkInterfaces();
    const ipList = [];
    
    // Collect all IPv4 addresses
    for (const name of Object.keys(interfaces)) {
        for (const iface of interfaces[name]) {
            if (iface.family === 'IPv4' && !iface.internal) {
                ipList.push({ name, address: iface.address });
            }
        }
    }
    
    // Sort: prioritize 10.x.x.x and 192.168.x.x (real network), deprioritize others (virtual)
    ipList.sort((a, b) => {
        const aIs10 = a.address.startsWith('10.');
        const bIs10 = b.address.startsWith('10.');
        const aIs192 = a.address.startsWith('192.168.') && !a.name.toLowerCase().includes('vmware') && !a.name.toLowerCase().includes('virtual');
        const bIs192 = b.address.startsWith('192.168.') && !b.name.toLowerCase().includes('vmware') && !b.name.toLowerCase().includes('virtual');
        
        if (aIs10 && !bIs10) return -1;
        if (!aIs10 && bIs10) return 1;
        if (aIs192 && !bIs192) return -1;
        if (!aIs192 && bIs192) return 1;
        return 0;
    });
    
    console.log(`========================================`);
    console.log(`  Shader Variant Collection Server`);
    console.log(`========================================`);
    console.log(`  Local:   http://localhost:${PORT}`);
    
    if (ipList.length > 0) {
        console.log(`  Network: http://${ipList[0].address}:${PORT}`);
        if (ipList.length > 1) {
            console.log(`  Other IPs:`);
            for (let i = 1; i < ipList.length; i++) {
                console.log(`    - http://${ipList[i].address}:${PORT} (${ipList[i].name})`);
            }
        }
    }
    
    console.log(`========================================`);
});
