# CtxSignTool

CtxSignTool is a command-line utility built on **CtxSignLib** for generating manifests, signing files, verifying detached signatures, creating development certificates, and extracting certificate pin information.

The tool is designed for deterministic software distribution workflows and integrates with CI/CD pipelines, build systems, and deployment tooling.

---

# Version

CtxSignTool version matches the underlying library version when possible.

Example:

```text
ctxsigntool --version
```

Output:

```text
CtxSignTool
Version: 1.1.0
Library Version: 1.1.0
```

---

# Pin Contract

CtxSignTool follows the immutable pin contract defined by **CtxSignLib**.

| Switch | Meaning |
|------|------|
| `--thumb` | Certificate SHA-1 thumbprint |
| `--pin` | Raw **SubjectPublicKeyInfo (SPKI)** public key material |
| `--pubpin` | SHA-256 hash of the SPKI public key |

### Accepted formats for `--pin`

The `--pin` switch accepts the **raw public key material** in any of these formats:

- PEM public key
- Base64 encoded SPKI
- Hex encoded SPKI

Example PEM:

```text
-----BEGIN PUBLIC KEY-----
MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE...
-----END PUBLIC KEY-----
```

Example Base64:

```text
MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE...
```

Example Hex:

```text
3059301306072A8648CE3D020106082A8648CE3D03010703420004...
```

### `--pubpin`

`--pubpin` is the SHA-256 digest of the SPKI public key.

Example:

```text
7f3a8b3e45c2b2f2f72f2df7bce1b4e9e5d7e3d50a75c0e7f0e0f3a5c4b9e01f
```

---

# Commands

## PrintPins

Extract pin values from a certificate.

```text
ctxsigntool --printpins --cert cert.cer
```

or

```text
ctxsigntool --printpins --pfx cert.pfx --pass password
```

Output includes:

```text
thumb
pin
pubpin
```

Optional switches:

```text
--json
--pretty
--out <file>
```

---

## MakeCert

Create a self-signed development certificate.

```text
ctxsigntool --makecert --out cert.pfx --pass password
```

Optional parameters:

```text
--cer cert.cer
--cn "Common Name"
--days 825
--eku code|doc|both
--rsa 3072
--pinsout pins.json
```

Tip:

```text
--pass env:ENVIRONMENT_VARIABLE
```

---

## Manifest

Generate a manifest describing files in a directory.

```text
ctxsigntool --manifest --root <directory>
```

Optional:

```text
--out ctxsign.json
```

---

## Sign

Sign a file.

Using PFX:

```text
ctxsigntool --sign --in file.exe --pfx cert.pfx --pass password
```

Using certificate thumbprint:

```text
ctxsigntool --sign --in file.exe --thumb <thumbprint>
```

Sign a manifest during creation:

```text
ctxsigntool --sign --manifest --root <directory>
```

---

## Verify

Verify a detached signature.

Using thumbprint:

```text
ctxsigntool --verify --in file.exe --thumb <thumbprint>
```

Using raw public key pin:

```text
ctxsigntool --verify --in file.exe --pin <spki-pem|base64|hex>
```

Using SHA-256 public key pin:

```text
ctxsigntool --verify --in file.exe --pubpin <spki-sha256-hex>
```

Verify using a certificate and derive the pin automatically:

```text
ctxsigntool --verify --in file.exe --cert cert.cer --pinmode pub
```

---

# Manifest Verification

Verify a signed manifest:

```text
ctxsigntool --verify --manifest ctxsign.json --thumb <thumbprint>
```

Verify a specific file against a manifest:

```text
ctxsigntool --verify --manifest ctxsign.json --root <dir> --in <file> --pubpin <hash>
```

---

# Exit Codes

| Code | Meaning |
|----|----|
| 0 | Success |
| 1 | Invalid arguments |
| 2 | Verification failed |
| 3 | File or certificate error |
| 4 | Internal error |

---

# Security Model

CtxSignTool produces **detached CMS / PKCS#7 signatures**.

The signer certificate is embedded inside the CMS signature so verification can pin the signer without relying on system certificate stores.

Verification can be performed using:

- certificate thumbprint
- raw public key pin
- SHA-256 public key pin

This enables deterministic verification in secure build pipelines and distributed systems.

---

# License

Apache 2.0

