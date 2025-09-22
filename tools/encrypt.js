#!/usr/bin/env node
// Usage: node tools/encrypt.js <input.json|txt> <output.enc> "<passphrase>"
const fs = require('fs');
const path = require('path');
const sodium = require('libsodium-wrappers-sumo');

(async () => {
  await sodium.ready;

  const SALTBYTES  = sodium.crypto_pwhash_SALTBYTES ?? 16;   // Argon2id salt
  const NONCEBYTES = sodium.crypto_aead_xchacha20poly1305_ietf_NPUBBYTES ?? 24;
  const KEYBYTES   = sodium.crypto_aead_xchacha20poly1305_ietf_KEYBYTES ?? 32;

  const [infile, outfile, pass] = process.argv.slice(2);
  if (!infile || !outfile || !pass) {
    console.error('Usage: node tools/encrypt.js <in> <out> "passphrase"');
    process.exit(1);
  }

  const data  = fs.readFileSync(infile);
  const salt  = sodium.randombytes_buf(SALTBYTES);
  const key   = sodium.crypto_pwhash(
    KEYBYTES, pass, salt,
    sodium.crypto_pwhash_OPSLIMIT_MODERATE,
    sodium.crypto_pwhash_MEMLIMIT_MODERATE,
    sodium.crypto_pwhash_ALG_DEFAULT // (= Argon2id)
  );
  const nonce  = sodium.randombytes_buf(NONCEBYTES);
  const cipher = sodium.crypto_aead_xchacha20poly1305_ietf_encrypt(data, null, null, nonce, key);

  fs.mkdirSync(path.dirname(outfile), { recursive: true });
  fs.writeFileSync(outfile, Buffer.concat([Buffer.from(salt), Buffer.from(nonce), Buffer.from(cipher)]));

  console.log(`Encrypted -> ${outfile} (size=${fs.statSync(outfile).size} bytes)`);
})();
