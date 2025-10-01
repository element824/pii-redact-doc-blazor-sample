# PII Detection & Redaction Blazor App

_A demonstration of Azure AI Foundry Language Service document-level PII redaction_

**Author:** Koushik Nagarajan ([@element824](https://github.com/element824))  
**Technology:** Blazor Server (.NET 8) + Azure AI Foundry Language Service  
**License:** MIT

---

A modern web application built with Blazor Server for detecting and redacting Personally Identifiable Information (PII) from PDF documents using Azure AI Foundry Language Service.

This application demonstrates the implementation of Microsoft's **"Detect and redact Personally Identifying Information in native documents"** feature from Azure AI Services Language Service.

## Official Documentation

This sample is based on the official Microsoft documentation: [Detect and redact Personally Identifying Information in native documents (preview)](https://learn.microsoft.com/en-us/azure/ai-services/language-service/personally-identifiable-information/how-to/redact-document-pii)

## Disclaimer

This sample code is provided for educational and demonstration purposes only. While created by a Microsoft employee based on official Azure AI Services documentation, this repository represents personal work and is not an official Microsoft product or service.

**Important Notes:**

- This code is provided "as-is" without warranty of any kind
- Microsoft does not provide official support for this sample
- Users are responsible for testing and validating the code in their own environments
- Always follow your organization's security and compliance policies when handling PII data
- Ensure proper data governance and privacy compliance (GDPR, CCPA, etc.) when processing personal information
- For official support and documentation, please refer to the Azure AI Services documentation.

## Features

- **PDF Upload**: Easy drag-and-drop interface for PDF document uploads
- **Azure AI Integration**: Seamless integration with Azure Blob Storage and AI Foundry Language Service
- **Real-time Processing**: Live status updates and progress tracking using Azure's asynchronous job processing
- **Native Document PII Detection**: Comprehensive detection of various PII categories including:
  - Personal names and organizations
  - Phone numbers and email addresses
  - Social Security Numbers (SSN)
  - Credit card numbers
  - IP addresses and dates
  - Physical addresses
- **Document Redaction**: Automatic masking of detected PII entities directly in PDF format
- **Dual Output**: Both JSON analysis results and redacted PDF documents
- **Secure Download**: Direct access to redacted documents via secure SAS URLs

## Technology Implementation

This application implements Microsoft's preview feature for **native document PII redaction**, which:

- Processes PDF documents directly without text extraction
- Maintains document formatting and structure
- Generates both analysis results and redacted documents
- Uses Azure AI Foundry's latest Language Service capabilities
- Leverages the `2024-11-15-preview` API for enhanced PII detection

## Prerequisites

- .NET 8.0 SDK
- Azure subscription with:
  - Azure Storage Account
  - Azure AI Foundry Language Service resource (with PII redaction preview features enabled)
- Visual Studio 2022 or VS Code

> **Note**: This application uses the preview API version `2024-11-15-preview` for document-level PII redaction capabilities.

## Setup Instructions

### 1. Clone and Setup

```bash
git clone <repository-url>
cd pii-manual/src
dotnet restore
```

### 2. Configure Azure Services

⚠️ **Security Note**: This project uses User Secrets to keep Azure credentials secure and out of version control.

The Azure credentials are already configured in User Secrets for this development environment:

- **Azure Storage Account**: `knpersonalraw01`
- **AI Foundry Language Service Endpoint**: `https://knlanguage01.cognitiveservices.azure.com`
- **API Version**: `2024-11-15-preview` (Preview API for document PII redaction)

> **Important**: This application requires an Azure AI Foundry Language Service resource with document-level PII redaction capabilities enabled.

If you need to set up your own Azure credentials, use the following commands:

```bash
# Initialize user secrets (already done for this project)
dotnet user-secrets init

# Set your Azure Storage connection string
dotnet user-secrets set "ConnectionStrings:AzureStorage" "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

# Set your Azure AI Foundry Language service credentials
dotnet user-secrets set "Azure:LanguageKey" "YOUR_API_KEY"
dotnet user-secrets set "Azure:LanguageEndpoint" "https://YOUR_RESOURCE.cognitiveservices.azure.com"
dotnet user-secrets set "Azure:ApiVersion" "2024-11-15-preview"
```

**Verify your secrets are set correctly:**

```bash
dotnet user-secrets list
```

### 3. Azure Storage Containers

The application uses the following containers in the `knpersonalraw01` storage account:

- **`piisource`** - for uploaded PDF documents (source files)
- **`piitarget`** - for redacted PDF documents (processed output)

These containers should already exist in the configured storage account. If you're using your own storage account, create these containers with private access level.

### 4. Verify Setup

Test that your configuration is working:

```bash
# Check that user secrets are configured
dotnet user-secrets list

# Build the project
dotnet build

# Run the application
dotnet run
```

## Running the Application

```bash
cd src
dotnet run
```

The application will be available at `http://localhost:5186`

**Quick Test:**

1. Navigate to `http://localhost:5186/pii-redaction`
2. Upload a PDF document
3. Click "Process Document" to start PII detection
4. Monitor the progress and download the redacted document when complete

## Project Structure

```
src/
├── Components/
│   ├── Layout/           # Application layout components
│   └── Pages/            # Blazor pages including PiiRedaction.razor
├── Models/               # Data models for PII processing
├── Services/             # Azure service integrations
│   ├── BlobStorageService.cs    # Azure Blob Storage operations
│   └── PiiDetectionService.cs   # Azure Language API integration
├── wwwroot/              # Static web assets
├── appsettings.json      # Configuration file
└── Program.cs            # Application entry point
```

## Usage

1. **Navigate** to the PII Redaction page at `/pii-redaction`
2. **Select** a PDF document to upload
3. **Configure** source and target containers (optional - defaults provided)
4. **Click** "Process Document" to start PII detection
5. **Monitor** the real-time processing status with progress indicators
6. **View Results** - See the redacted text content in the browser
7. **Download JSON Results** - Get the complete PII analysis data as JSON
8. **Download Document** - Access the redacted PDF document directly from Azure Storage

## Key Components

### BlobStorageService

- Handles file uploads to Azure Blob Storage
- Generates SAS URLs for secure document access
- Manages container operations

### PiiDetectionService

- Submits PII detection jobs to Azure AI Foundry Language Service API
- Polls job status for completion using the document analysis endpoint
- Extracts redacted document URLs from API responses
- Handles API communication and error management
- Implements the preview API for native document PII redaction

### PiiRedaction Component

- Interactive Blazor component for the main UI
- Real-time status updates using timers
- File upload handling and progress tracking
- Dual download options: JSON results and redacted documents

## Configuration Options

### PII Categories Detected

The application detects and redacts the following PII categories:

- **Person** - Personal names and identifiers
- **Organization** - Company and organization names
- **PhoneNumber** - Phone numbers in various formats
- **Email** - Email addresses
- **Address** - Physical addresses and locations
- **USSocialSecurityNumber** - US Social Security Numbers
- **CreditCardNumber** - Credit card numbers
- **IPAddress** - IP addresses (IPv4/IPv6)
- **DateTime** - Date and time information
- **Age** - Age-related information

**Note**: These are the categories currently supported by Azure AI Foundry Language Service API for document-level PII detection and redaction.

### Redaction Policy

- **Entity Mask**: Replaces PII with entity type labels (default)
- Configurable through the PiiParameters class

## Security Considerations

1. **Never commit API keys** to version control
2. **Use User Secrets** for development
3. **Use Azure Key Vault** for production
4. **SAS tokens** have configurable expiration times
5. **HTTPS** should be enabled in production

## Troubleshooting

### Common Issues

1. **SAS Token Expiry**: Ensure SAS tokens are valid for at least 24 hours
2. **Container Access**: Verify storage account connection and container existence
3. **API Rate Limits**: Azure AI Foundry Language Service may have rate limits
4. **File Size Limits**: Default limit is 10MB per file
5. **Preview API**: Ensure your Language Service resource has preview features enabled

### Debug Mode

Run with detailed logging:

```bash
dotnet run --environment Development
```

## API Reference

The application integrates with:

- **Azure Blob Storage API** for file operations and SAS URL generation
- **Azure AI Foundry Language Service API** for document-level PII detection and redaction
- **Blazor Server** for real-time UI updates and interactive components

### Related Documentation

- [Azure AI Foundry Language Service Documentation](https://learn.microsoft.com/en-us/azure/ai-services/language-service/)
- [Detect and redact Personally Identifying Information in native documents (preview)](https://learn.microsoft.com/en-us/azure/ai-services/language-service/personally-identifiable-information/how-to/redact-document-pii)
- [PII entity categories](https://learn.microsoft.com/en-us/azure/ai-services/language-service/personally-identifiable-information/concepts/entity-categories)

## Author

**Koushik Nagarajan**  
GitHub: [@element824](https://github.com/element824)  
Software Engineer & AI Solutions Developer

## License

MIT License

Copyright (c) 2025 Koushik Nagarajan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

_This project demonstrates the implementation of Azure AI Foundry Language Service's document-level PII redaction capabilities using modern .NET technologies._

**Connect with the author:**  
🔗 GitHub: [@element824](https://github.com/element824)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Support

For issues and questions:

- Check the troubleshooting section
- Review Azure service documentation
- Create an issue in the repository
