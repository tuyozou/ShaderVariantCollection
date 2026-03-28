const express = require('express');
const fs = require('fs');
const path = require('path');

const app = express();
const PORT = 8880;
const DATA_FILE = path.join(__dirname, 'data.json');

// Middleware
app.use(express.json({ limit: '10mb' }));
app.use(express.static(path.join(__dirname, 'public')));

// In-memory data and deduplication set
let dataList = [];
let messageSet = new Set();

// Load data from JSON file on startup
function loadData() {
    try {
        if (fs.existsSync(DATA_FILE)) {
            const content = fs.readFileSync(DATA_FILE, 'utf-8');
            dataList = JSON.parse(content);
            // Rebuild the deduplication set
            messageSet = new Set(dataList.map(item => item.message));
            console.log(`Loaded ${dataList.length} records from data.json`);
        }
    } catch (err) {
        console.error('Error loading data:', err);
        dataList = [];
        messageSet = new Set();
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
    const { message } = req.body;
    
    if (!message) {
        return res.status(400).json({ ok: false, error: 'message is required' });
    }
    
    // Check for duplicate
    if (messageSet.has(message)) {
        return res.json({ ok: false, exists: true });
    }
    
    // Add new entry
    const entry = {
        message: message,
        time: new Date().toISOString()
    };
    
    dataList.push(entry);
    messageSet.add(message);
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
    messageSet = new Set();
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
