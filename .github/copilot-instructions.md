# PowerAppsDevMCP - Model Context Protocol Server

PowerAppsDevMCP is a .NET 8.0 Model Context Protocol (MCP) server built using Microsoft's official MCP SDK approach. It provides tools for basic text processing and demonstrates the MCP protocol implementation.

**ALWAYS follow these instructions first and only fallback to additional search and context gathering if the information here is incomplete or found to be in error.**

## Working Effectively

### Prerequisites and Setup
- Ensure .NET 8.0 SDK is installed: `dotnet --version` (should show 8.0.x)
- Navigate to the repository root: `/home/runner/work/PowerAppsDevMCP/PowerAppsDevMCP`
- The main project is in the `McpServer/` subdirectory

### Building and Testing
- **NEVER CANCEL builds or tests** - All operations complete quickly (under 15 seconds)
- Navigate to McpServer directory: `cd McpServer`
- Restore dependencies: `dotnet restore` (takes ~8 seconds)
- Build the project: `dotnet build` (takes ~8 seconds, timeout: 60 seconds)
- Verify formatting: `dotnet format --verify-no-changes` (takes ~7 seconds, timeout: 60 seconds)
- Run integration tests: `./test_server.sh` from repo root (takes ~10 seconds, timeout: 60 seconds)

### Running the MCP Server
- Start the server: `dotnet run` from `McpServer/` directory
- Server listens on stdin/stdout for JSON-RPC MCP protocol messages
- Logs are redirected to stderr (good for MCP protocol compliance)
- Use Ctrl+C to stop the server

## Validation Requirements

### Manual Testing Scenarios
**ALWAYS test these complete scenarios after making changes:**

1. **Complete MCP Protocol Flow**:
   ```bash
   cd McpServer
   (echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0"}}}'; sleep 2; echo '{"jsonrpc":"2.0","method":"initialized","params":{}}'; sleep 2; echo '{"jsonrpc":"2.0","id":2,"method":"tools/list"}'; sleep 2; echo '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"hello","arguments":{"message":"Test Message"}}}'; sleep 2) | timeout 30 dotnet run
   ```
   Expected: Should return proper JSON-RPC responses including tool list and "Hello from PowerApps MCP Server: Test Message"

2. **Tool Functionality Test**:
   - hello tool: `{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"hello","arguments":{"message":"PowerApps"}}}`
   - reverse_echo tool: `{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"reverse_echo","arguments":{"message":"PowerApps"}}}`
   - Expected outputs: "Hello from PowerApps MCP Server: PowerApps" and "sppArewoP"

3. **Integration Test**: Run `./test_server.sh` from repo root - must pass completely

### Pre-commit Validation
Always run these commands before committing changes:
1. `dotnet build` (must succeed with 0 warnings)
2. `dotnet format --verify-no-changes` (must show "Formatted 0 files")
3. `./test_server.sh` (must show "Testing complete!")

## Architecture and Code Structure

### Key Projects and Files
- `McpServer/McpServer.csproj` - Main project file with dependencies
- `McpServer/Program.cs` - Application entry point and tool definitions
- `test_server.sh` - Integration test script
- `.gitignore` - Excludes .NET build artifacts

### Dependencies (NuGet Packages)
- `ModelContextProtocol` (0.3.0-preview.4) - Official Microsoft MCP SDK
- `Microsoft.Extensions.Hosting` (9.0.9) - .NET hosting patterns and DI
- `System.Text.Json` (9.0.9) - JSON serialization

### Tool Implementation
Tools are defined using Microsoft's attribute-based approach:
```csharp
[McpServerToolType]
public static class PowerAppsTools
{
    [McpServerTool, Description("Tool description")]
    public static string ToolName(string parameter) => "result";
}
```

**IMPORTANT**: Tool names are automatically converted to lowercase with underscores (e.g., "HelloWorld" becomes "hello_world")

### Current Available Tools
1. **hello** - Returns greeting message
   - Parameter: `message` (string, optional, default: "World")
   - Example: `"Hello from PowerApps MCP Server: {message}"`

2. **reverse_echo** - Returns reversed input string
   - Parameter: `message` (string, required)  
   - Example: "PowerApps" → "sppArewoP"

## Common Development Tasks

### Adding New Tools
1. Add static method to `PowerAppsTools` class in `Program.cs`
2. Use `[McpServerTool, Description("...")]` attributes
3. Tool names will be lowercase with underscores automatically
4. Build and test with complete MCP protocol flow
5. Update this documentation with new tool details

### Debugging MCP Protocol Issues
- Check stderr output for detailed logs when running `dotnet run`
- Verify JSON-RPC message format compliance
- Use the test script pattern for consistent message sending
- Remember: stdout = MCP responses, stderr = application logs

### Performance Characteristics
- Build time: ~8 seconds (very fast)
- Startup time: ~2 seconds
- MCP protocol response time: ~1 second per message
- All operations complete quickly - no long-running processes

### Troubleshooting
- **Build fails**: Run `dotnet restore` first, then `dotnet build`
- **Test failures**: Ensure server is not already running, check JSON syntax
- **No MCP responses**: Verify stdin/stdout handling, check stderr for errors
- **Tool not found**: Check tool name casing (use lowercase with underscores)

## Repository Structure Quick Reference
```
/home/runner/work/PowerAppsDevMCP/PowerAppsDevMCP/
├── .github/copilot-instructions.md  # This file
├── .gitignore                       # .NET artifacts exclusion
├── README.md                        # Project documentation
├── test_server.sh                   # Integration tests
└── McpServer/
    ├── McpServer.csproj            # Project dependencies
    └── Program.cs                  # MCP server and tools
```

## Critical Notes
- **NEVER CANCEL** any build or test commands - they complete in under 15 seconds
- Always use lowercase tool names when calling via MCP protocol
- Test complete MCP protocol flows, not just individual components
- Validate changes with the integration test script before committing
- MCP responses go to stdout, application logs go to stderr (this is correct)