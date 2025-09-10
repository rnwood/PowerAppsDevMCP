# PowerAppsDevMCP

A hello world MCP (Model Context Protocol) server built with .NET 8.0. This server demonstrates the basic implementation of the MCP protocol and provides a simple "hello" tool for testing.

## Features

- **MCP Protocol Support**: Implements the MCP 2024-11-05 protocol version
- **Hello World Tool**: Provides a simple greeting tool that can personalize messages
- **JSON-RPC Communication**: Communicates via JSON-RPC 2.0 over stdin/stdout
- **Structured Logging**: Uses Microsoft.Extensions.Logging for comprehensive logging

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Building the Project

```bash
cd PowerAppsDevMCP.Server
dotnet restore
dotnet build
```

### Running the Server

```bash
cd PowerAppsDevMCP.Server
dotnet run
```

The server will start and listen for MCP protocol messages on stdin, responding on stdout.

## Usage

The server implements the following MCP protocol methods:

- `initialize` - Initialize the server and exchange capabilities
- `initialized` - Notification that initialization is complete
- `tools/list` - List available tools
- `tools/call` - Call a specific tool

### Available Tools

#### `hello`

Returns a personalized greeting message.

**Parameters:**
- `name` (optional): Name to include in the greeting. Defaults to "World".

**Example:**
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
        "text": "Hello, PowerApps Developer! This is a greeting from the PowerApps Dev MCP Server."
      }
    ]
  }
}
```

## Project Structure

```
PowerAppsDevMCP.Server/
├── Models/
│   ├── JsonRpcMessage.cs    # JSON-RPC protocol models
│   └── McpModels.cs         # MCP-specific models
├── Services/
│   └── McpServer.cs         # Main MCP server implementation
├── Program.cs               # Application entry point
└── PowerAppsDevMCP.Server.csproj
```

## Development

This project serves as a foundation for building more sophisticated MCP servers for PowerApps development tools and utilities.