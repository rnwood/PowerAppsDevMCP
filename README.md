# PowerAppsDevMCP

A simple hello world MCP (Model Context Protocol) server built with .NET 8.0, following Microsoft's recommended approach for building MCP servers in C#.

## Features

- **MCP Protocol Compliance**: Implements MCP 2024-11-05 protocol specification
- **Hello World Tool**: Simple greeting tool demonstrating MCP tool functionality  
- **JSON-RPC 2.0**: Standard JSON-RPC communication over stdin/stdout
- **Clean Architecture**: Single-file implementation following .NET conventions

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

## Usage

The server implements the core MCP protocol methods:

- `initialize` - Initialize server and exchange capabilities
- `initialized` - Notification that initialization is complete  
- `tools/list` - List available tools
- `tools/call` - Call a specific tool

### Hello Tool

The server provides a single "hello" tool that returns personalized greetings.

**Parameters:**
- `name` (optional): Name to include in greeting (defaults to "World")

**Example Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "hello",
    "arguments": {
      "name": "PowerApps Developer"
    }
  }
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Hello, PowerApps Developer! This is a greeting from the PowerApps MCP Server."
      }
    ]
  }
}
```

## Testing

Run the included test script to verify all functionality:

```bash
./test_server.sh
```

This tests all implemented MCP methods and demonstrates the hello tool in action.

## Implementation

This MCP server follows Microsoft's recommended approach using:
- Standard .NET 8.0 console application
- Built-in `System.Text.Json` for JSON handling
- `JsonNode` for flexible JSON manipulation
- Simple, single-file architecture for clarity

The implementation demonstrates the core concepts needed to build MCP servers in .NET without unnecessary complexity.