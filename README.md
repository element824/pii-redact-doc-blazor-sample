# PII Detection & Redaction Blazor App

**Author:** Koushik Nagarajan ([@element824](https://github.com/element824))  
**Technology:** Blazor Server (.NET 8) + Azure AI Foundry Language Service  
**License:** MIT

---

A modern web application built with Blazor Server for detecting and redacting Personally Identifiable Information (PII) from PDF documents using Azure AI Foundry Language Service.

## Disclaimer

This sample code is provided for educational and demonstration purposes only. While created by a Microsoft employee based on official Azure AI Services documentation, this repository represents personal work and is not an official Microsoft product or service.

**Important Notes:**

- This code is provided "as-is" without warranty of any kind
- Microsoft does not provide official support for this sample
- Users are responsible for testing and validating the code in their own environments
- Always follow your organization's security and compliance policies when handling PII data
- Ensure proper data governance and privacy compliance (GDPR, CCPA, etc.) when processing personal information

## Prerequisites

- .NET 8.0 SDK
- Azure subscription with:
  - Azure Storage Account
  - Azure AI Foundry Language Service resource (with PII redaction preview features enabled)

## Quick Setup

### 1. Clone and Setup

```bash
git clone <repository-url>
cd pii-redact-doc-blazor-sample
dotnet restore
```

### 2. Configure Azure Credentials

```bash
# Set your Azure Storage connection string
dotnet user-secrets set "ConnectionStrings:AzureStorage" "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

# Set your Azure AI Language service credentials
dotnet user-secrets set "Azure:LanguageKey" "YOUR_API_KEY"
dotnet user-secrets set "Azure:LanguageEndpoint" "https://YOUR_RESOURCE.cognitiveservices.azure.com"
dotnet user-secrets set "Azure:ApiVersion" "2024-11-15-preview"
```

### 3. Create Storage Containers

Create these containers in your Azure storage account:

- `piisource` - for uploaded PDF documents
- `piitarget` - for redacted PDF documents

### 4. Run the Application

```bash
dotnet run
```

Navigate to `http://localhost:5186`

## Usage

1. Upload a PDF document
2. Click "Process Document" to start PII detection
3. Monitor processing status
4. View and download redacted results

## Features

- PDF upload with drag-and-drop interface
- Real-time processing with progress tracking
- Native document PII detection and redaction
- Download both JSON results and redacted PDF documents
- Secure Azure integration with SAS URLs

## Documentation

For detailed information, refer to the official Microsoft documentation:

- [Detect and redact Personally Identifying Information in native documents (preview)](https://learn.microsoft.com/en-us/azure/ai-services/language-service/personally-identifiable-information/how-to/redact-document-pii)
- [Azure AI Foundry Language Service Documentation](https://learn.microsoft.com/en-us/azure/ai-services/language-service/)

---

## Author

**Koushik Nagarajan**  
GitHub: [@element824](https://github.com/element824)  
Software Engineer & AI Solutions Developer

## License

MIT License - Copyright (c) 2025 Koushik Nagarajan

_This project demonstrates the implementation of Azure AI Foundry Language Service's document-level PII redaction capabilities using modern .NET technologies._

**Connect with the author:**  
🔗 GitHub: [@element824](https://github.com/element824)
