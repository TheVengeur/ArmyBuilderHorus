#!/usr/bin/env node
// Usage: node tools/decrypt.js <input.enc> <output.json|txt> "<passphrase>"
const fs = require('fs');
const sodium = require('libsodium-wrappers-sumo');

(async () => {
  await sodium.ready;

  const SALTBYTES  = sodium.crypto_pwhash_SALTBYTES ?? 16;
  const NONCEBYTES = sodium.crypto_aead_xchacha20poly1305_ietf_NPUBBYTES ?? 24;
  const ABYTES     = sodium.crypto_aead_xchacha20poly1305_ietf_ABYTES ?? 16;
  const KEYBYTES   = sodium.crypto_aead_xchacha20poly1305_ietf_KEYBYTES ?? 32;

  const [infile, outfile, pass] = process.argv.slice(2);
  if (!infile || !outfile || !pass) {
    console.error('Usage: node tools/decrypt.js <in> <out> "passphrase"');
    process.exit(1);
  }

  const raw = fs.readFileSync(infile);
  if (raw.length < SALTBYTES + NONCEBYTES + ABYTES) {
    console.error('Blob too small / corrupted');
    process.exit(2);
  }

  const salt   = raw.slice(0, SALTBYTES);
  const nonce  = raw.slice(SALTBYTES, SALTBYTES + NONCEBYTES);
  const cipher = raw.slice(SALTBYTES + NONCEBYTES);

  const key = sodium.crypto_pwhash(
    KEYBYTES, pass, salt,
    sodium.crypto_pwhash_OPSLIMIT_MODERATE,
    sodium.crypto_pwhash_MEMLIMIT_MODERATE,
    sodium.crypto_pwhash_ALG_DEFAULT
  );

  try {
    const plain = sodium.crypto_aead_xchacha20poly1305_ietf_decrypt(null, cipher, null, nonce, key);
    fs.writeFileSync(outfile, Buffer.from(plain));
    console.log(`Decrypted -> ${outfile}`);
  } catch {
    console.error('Decryption failed — wrong key or corrupted file');
    process.exit(3);
  }
})();

