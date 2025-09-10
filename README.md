# PowerAppsDevMCP

A Model Context Protocol (MCP) server built with .NET 8.0, following Microsoft's official approach for building MCP servers in C#.

## Features

- **Official MCP SDK**: Uses the official `ModelContextProtocol` NuGet package
- **Microsoft.Extensions.Hosting**: Proper .NET service patterns and dependency injection
- **Attribute-based Tools**: Clean tool definition using Microsoft's attribute approach
- **MCP Protocol Compliance**: Implements MCP 2024-11-05 protocol specification
- **Background Services**: Uses BackgroundService for stdin/stdout handling

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Building and Running

```bash
cd McpServer
dotnet build
dotnet run
```

The server starts immediately and listens for MCP protocol messages on stdin.

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

## Testing

Run the included test script to verify all functionality:

```bash
./test_server.sh
```

This tests all implemented MCP methods and demonstrates the tools in action.

## Dependencies

- ModelContextProtocol (0.3.0-preview.4)
- Microsoft.Extensions.Hosting (9.0.9)

## Implementation Notes

This implementation follows Microsoft's recommended approach as outlined in the [official DevBlog](https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/), using the official MCP C# SDK for robust, maintainable MCP server development.