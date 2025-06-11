# GhostText 🕵️‍♂️ - Unicode Homoglyph Steganography

![Language](https://img.shields.io/badge/language-C%23%207.3-blueviolet)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-Active-brightgreen)

**GhostText** is a steganographic tool that hides secret messages inside regular text by replacing certain Latin characters with visually identical Unicode homoglyphs. The result is a text that looks unchanged to humans, but contains a concealed message extractable only with the proper method.

---

## 🧩 Features

- Hides a message in plain text using Unicode homoglyphs.
- SHA256 integrity check to detect tampering.
- Reads and writes from files (carrier, secret, output, hash).
- No third-party dependencies.
- Compatible with **C# 7.3**.

---

## 📦 Requirements

- .NET Framework or .NET Core supporting C# 7.3
- Windows terminal (UTF-8 support recommended)

---

## 🛠️ Build

To build the project:

```bash
dotnet build
```

## 🚀 Usage

### Encode a message:

GhostText.exe encode carrier.txt secret.txt output.txt hash.txt

    carrier.txt: File containing the base text where the message will be hidden.

    secret.txt: File containing the message you want to hide.

    output.txt: Output file with the hidden message embedded.

    hash.txt: Output file storing the SHA256 hash of the steganographic text.

### Decode a message:

GhostText.exe decode output.txt hash.txt

    output.txt: File with the embedded secret message.

    hash.txt: Original hash to verify integrity before decoding.

## 📄 Example

GhostText.exe encode intro.txt secret.txt stegano.txt hash.txt
GhostText.exe decode stegano.txt hash.txt

If the hashes match:
```
Expected SHA256 hash: XXXXXXXXXXXXXXXXXXXXXXXXXX
Computed SHA256 hash: XXXXXXXXXXXXXXXXXXXXXXXXXX
Integrity check passed.
Extracted Message:
hello_world
```

## ⚠️ Notes

    Only certain letters (a, e, o, i, c, d, m, n, r, s, t) are used for encoding.

    The carrier text must contain enough of these letters to encode the entire message.

    If the message is too long, the program will abort with an error.

## 🧠 Disclaimer

This project is intended for educational and experimental purposes only. It does not provide strong cryptographic guarantees and should not be used for securing sensitive or personal data.
