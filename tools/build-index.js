#!/usr/bin/env node
// Usage: node tools/build-index.js --base=https://raw.githubusercontent.com/<USER>/<REPO>/main/packs --version=2025.09.22
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

function arg(name, def=null){
  const m = process.argv.find(a => a.startsWith(`--${name}=`));
  return m ? m.split('=')[1] : def;
}

const base = arg('base');
const version = arg('version', new Date().toISOString().slice(0,10));
if(!base){ console.error('Missing --base=...'); process.exit(1); }

const root = path.resolve('packs');
function listEnc(dir='') {
  const abs = path.join(root, dir);
  return fs.readdirSync(abs).flatMap(f => {
    const rel = path.join(dir, f);
    const stat = fs.statSync(path.join(root, rel));
    if (stat.isDirectory()) return listEnc(rel);
    return f.endsWith('.enc') ? [rel.replace(/\\/g,'/')] : [];
  });
}

const files = listEnc();
const out = {
  format: 1,
  version,
  base_url: base.endsWith('/') ? base : base + '/',
  files: files.map(rel => {
    const buf = fs.readFileSync(path.join(root, rel));
    const sha = crypto.createHash('sha256').update(buf).digest('hex');
    return { path: rel, bytes: buf.length, sha256: sha };
  })
};

fs.writeFileSync(path.join(root, 'packs.index.json'), JSON.stringify(out, null, 2));
console.log(`Wrote packs/packs.index.json with ${out.files.length} files.`);
