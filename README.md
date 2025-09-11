# PowerAppsDevMCP

A Model Context Protocol (MCP) server built with .NET 8.0, following Microsoft's official approach for building MCP servers in C#.

## Features

- **Official MCP SDK**: Uses the official `ModelContextProtocol` NuGet package
- **Microsoft.Extensions.Hosting**: Proper .NET service patterns and dependency injection
- **Attribute-based Tools**: Clean tool definition using Microsoft's attribute approach
- **MCP Protocol Compliance**: Implements MCP 2024-11-05 protocol specification
- **Background Services**: Uses BackgroundService for stdin/stdout handling
- **Dataverse Integration**: Connect to Microsoft Dataverse using DefaultAzureCredential authentication
- **WhoAmI Support**: Query authenticated user information from Dataverse environments

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Building and Running

```bash
cd McpServer
dotnet build
dotnet run -- --environment-url https://yourorg.crm.dynamics.com
```

The server starts immediately and listens for MCP protocol messages on stdin.

**Command Line Arguments:**

- `--environment-url` (optional): The Dataverse environment URL for WhoAmI operations (e.g., https://yourorg.crm.dynamics.com)

If no environment URL is provided, WhoAmI calls will return an error message indicating that the URL needs to be configured.

## Authentication

The WhoAmI tool uses Azure DefaultAzureCredential for authentication, which automatically tries multiple authentication methods in order:

1. Environment variables (Azure CLI, service principal)
2. Managed Identity (when running in Azure)
3. Visual Studio / VS Code authentication
4. Azure CLI authentication
5. Azure PowerShell authentication

This approach provides secure, passwordless authentication without requiring browser interaction.

**Setup Options:**

- **Azure CLI**: Run `az login` before using the tool
- **Environment Variables**: Set `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`
- **Managed Identity**: When running in Azure (App Service, Functions, VMs)

## Architecture

This implementation follows the Microsoft DevBlog approach:

1. **Host.CreateApplicationBuilder** - Standard Microsoft hosting pattern
2. **AddMcpServer()** - Official MCP SDK server registration  
3. **WithStdioServerTransport()** - Standard transport for MCP protocol
4. **WithToolsFromAssembly()** - Automatic tool discovery using attributes

### Tool Definition

Tools are defined using Microsoft's attribute-based approach:

```csharp
[McpServerToolType]
public static class PowerAppsTools
{
    [McpServerTool, Description("Returns a hello world message from the PowerApps MCP Server.")]
    public static string Hello(string message = "World") => $"Hello from PowerApps MCP Server: {message}";

    [McpServerTool, Description("Echoes the message back in reverse from the PowerApps MCP Server.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
```

## Usage

The server implements the core MCP protocol methods:

- `initialize` - Initialize server and exchange capabilities
- `initialized` - Notification that initialization is complete  
- `tools/list` - List available tools
- `tools/call` - Call a specific tool

### Available Tools

1. **Hello** - Returns a greeting message
   - Parameter: `message` (string, optional, default: "World")

2. **ReverseEcho** - Returns the input message reversed
   - Parameter: `message` (string, required)

3. **WhoAmI** - Connects to Microsoft Dataverse and returns authenticated user information
   - Environment URL configured via `--environment-url` command line argument when starting the server
   - Uses Azure DefaultAzureCredential for authentication (no browser interaction required)
   - Returns: JSON with userId, businessUnitId, organizationId, and connection details

### Example Request

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "Hello",
    "arguments": {
      "message": "PowerApps Developer"
    }
  }
}
```

### WhoAmI Example

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "who_am_i",
    "arguments": {}
  }
}
```

**Note:** The environment URL is now configured when starting the server using the `--environment-url` command line argument for improved security.

**Example Response:**
```json
{
  "success": true,
  "userId": "12345678-1234-1234-1234-123456789012",
  "businessUnitId": "87654321-4321-4321-4321-210987654321",
  "organizationId": "abcdef12-3456-7890-abcd-ef1234567890",
  "environmentUrl": "https://yourorg.crm.dynamics.com",
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

## Testing

Run the included test script to verify all functionality:

```bash
./test_server.sh
```

This tests all implemented MCP methods and demonstrates the tools in action.

## Dependencies

- ModelContextProtocol (0.3.0-preview.4)
- Microsoft.Extensions.Hosting (9.0.9)
- Microsoft.PowerPlatform.Dataverse.Client (1.1.32)
- Azure.Identity (1.12.1)
- System.CommandLine (2.0.0-beta4.22272.1)

## Implementation Notes

This implementation follows Microsoft's recommended approach as outlined in the [official DevBlog](https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/), using the official MCP C# SDK for robust, maintainable MCP server development.