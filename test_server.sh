#!/bin/bash

# Test script for the MCP server
echo "Testing PowerApps MCP Server..."

# Build the server
echo "Building server..."
cd McpServer
dotnet build -q
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Function to test a message
test_message() {
    echo "Testing: $1"
    echo "$2" | dotnet run 2>/dev/null | head -1
    echo ""
}

# Test initialize
test_message "Initialize" '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0"}}}'

# Test tools/list
test_message "List tools" '{"jsonrpc":"2.0","id":2,"method":"tools/list"}'

# Test tools/call with hello
test_message "Call hello tool" '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"hello","arguments":{"name":"PowerApps Developer"}}}'

# Test tools/call with hello (no name)
test_message "Call hello tool (no name)" '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"hello","arguments":{}}}'

echo "Testing complete!"