# Secure Communication System with Certificate Authority

A comprehensive **secure peer-to-peer messaging system** built with .NET 8.0, implementing PKI (Public Key Infrastructure), digital certificates, and encrypted communication protocols.

## 📋 Project Overview

This project demonstrates a complete secure communication system consisting of three main components:

1. **Certificate Authority (CA)** - Issues and signs digital certificates
2. **Client 1** - Listens for incoming peer connections
3. **Client 2** - Initiates connection to Client 1

The system implements industry-standard cryptographic protocols for secure message exchange between two parties.

## 🔐 Security Features

### Cryptographic Algorithms
- **RSA-2048** - Asymmetric encryption for key exchange and digital signatures
- **AES-256** - Symmetric encryption for message communication
- **SHA-256** - Hash function for key derivation and data integrity
- **PKCS#1 v1.5** - Digital signature padding scheme
- **OAEP** - Optimal Asymmetric Encryption Padding for RSA encryption

### Security Protocol Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          STAGE 1: PKI SETUP                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│  1. CA generates RSA key pair (Public Key, Private Key)                     │
│  2. Clients generate their own RSA key pairs                                │
│  3. Clients request certificates from CA                                    │
│  4. CA signs certificates with its private key                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                       STAGE 2: CERTIFICATE EXCHANGE                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  1. Clients exchange certificates                                           │
│  2. Each client verifies peer's certificate using CA's public key           │
│  3. Invalid certificates are rejected                                       │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                      STAGE 3: MASTER KEY ESTABLISHMENT                      │
├─────────────────────────────────────────────────────────────────────────────┤
│  1. Client 1 generates N1 (32-byte random nonce)                            │
│  2. Client 1 encrypts N1 with Client 2's public key and sends               │
│  3. Client 2 generates N2 (32-byte random nonce)                            │
│  4. Client 2 encrypts N2 with Client 1's public key and sends               │
│  5. Master Key Km = SHA256(N1 || N2)                                        │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                     STAGE 4: SESSION KEY ESTABLISHMENT                      │
├─────────────────────────────────────────────────────────────────────────────┤
│  1. Client 1 generates N3, encrypts with Km using AES, sends                │
│  2. Client 2 generates N4, encrypts with Km using AES, sends                │
│  3. Session Key Ks = SHA256(N3 || N4)                                       │
│  4. All subsequent messages encrypted with AES using Ks                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🏗️ Project Structure

```
networksecurity/
├── ag_guvenligi_bitirme.sln     # Solution file
├── CA/                           # Certificate Authority application
│   ├── MainForm.cs              # CA server logic
│   ├── MainForm.Designer.cs     # UI design
│   ├── Program.cs               # Entry point
│   └── CA.csproj                # Project file
├── Client1/                      # Client 1 application (Listener)
│   ├── MainForm.cs              # Client logic with listening capability
│   ├── MainForm.Designer.cs     # UI design
│   ├── Program.cs               # Entry point
│   └── Client1.csproj           # Project file
├── Client2/                      # Client 2 application (Connector)
│   ├── MainForm.cs              # Client logic with connection capability
│   ├── MainForm.Designer.cs     # UI design
│   ├── Program.cs               # Entry point
│   └── Client2.csproj           # Project file
└── Shared/                       # Shared library
    ├── Certificate.cs           # Digital certificate model
    ├── CryptoHelper.cs          # Cryptographic utility functions
    └── Shared.csproj            # Project file
```

## ⚙️ Requirements

- **.NET 8.0 SDK** or later
- **Windows** (Windows Forms application)
- **Visual Studio 2022** or **VS Code** with C# extension

## 🚀 Getting Started

### Build the Solution

```bash
dotnet build ag_guvenligi_bitirme.sln --configuration Release
```

### Running the Applications

**Step 1:** Start the Certificate Authority
```bash
dotnet run --project CA
```
- Set a port (e.g., 5000)
- Click "Start Server"

**Step 2:** Start Client 1 (Listener)
```bash
dotnet run --project Client1
```
- Enter Client ID (e.g., "Alice")
- Set CA Server address and port
- Set Listen Port (e.g., 5001)
- Click "Initialize"

**Step 3:** Start Client 2 (Connector)
```bash
dotnet run --project Client2
```
- Enter Client ID (e.g., "Bob")
- Set CA Server address and port
- Set Peer Server address and Client 1's listen port
- Click "Connect"

### Multi-Computer Setup

For running on separate machines:
1. Replace `127.0.0.1` with actual IP addresses
2. Ensure firewall allows the specified ports
3. All machines must have network connectivity

## 📊 Application Screenshots

### Certificate Authority
- Displays server status (Running/Stopped)
- Shows certificates issued count
- Lists all connected clients
- Logs all certificate requests and issuances

### Client Applications
- Real-time connection status
- Certificate verification status
- Session key establishment status
- Encrypted chat interface
- Detailed security protocol logs

## 🔧 Technical Details

### Certificate Structure
```json
{
  "SubjectID": "ClientName",
  "PublicKey": "Base64EncodedRSAPublicKey",
  "Validity": "2025-12-28T00:00:00Z",
  "SerialNumber": "UniqueGUID",
  "Signature": "CADigitalSignature"
}
```

### Message Encryption
- Each message is encrypted using AES-256-CBC
- Random IV generated for each message
- Format: `[IV (16 bytes)][Encrypted Message]`

### Key Derivation
- Uses SHA-256 for deriving keys from nonces
- Ensures forward secrecy through unique session keys

## 📝 Course Information

**Course:** BIM 437 - Bilgisayar ve Ağ Güvenliği (Computer and Network Security)  
**Project Type:** Term Project  
**Framework:** .NET 8.0 with Windows Forms

## 📄 License

This project is developed for educational purposes as part of the Computer and Network Security course.

## 👥 Author

Taha Emre

---

*This project demonstrates practical implementation of public key infrastructure, digital certificates, and secure communication protocols.*
