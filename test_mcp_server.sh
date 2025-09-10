#!/bin/bash

# Test script for PowerApps Dev MCP Server
set -e

echo "Building the MCP Server..."
cd PowerAppsDevMCP.Server
dotnet build --verbosity quiet

echo "Testing MCP Server functionality..."

# Create a temporary directory for test files
TEST_DIR="/tmp/mcp_test_$$"
mkdir -p "$TEST_DIR"

# Test 1: Initialize
echo "Test 1: Initialize server"
cat > "$TEST_DIR/initialize.json" << 'EOF'
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0.0"}}}
EOF

# Test 2: Initialized notification
echo "Test 2: Send initialized notification"
cat > "$TEST_DIR/initialized.json" << 'EOF'
{"jsonrpc":"2.0","method":"initialized","params":{}}
EOF

# Test 3: List tools
echo "Test 3: List available tools"
cat > "$TEST_DIR/list_tools.json" << 'EOF'
{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
EOF

# Test 4: Call hello tool without parameters
echo "Test 4: Call hello tool (default)"
cat > "$TEST_DIR/call_hello_default.json" << 'EOF'
{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"hello","arguments":{}}}
EOF

# Test 5: Call hello tool with name parameter
echo "Test 5: Call hello tool with name"
cat > "$TEST_DIR/call_hello_name.json" << 'EOF'
{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"hello","arguments":{"name":"PowerApps Developer"}}}
EOF

# Combine all test messages
cat "$TEST_DIR"/*.json > "$TEST_DIR/all_tests.json"

# Run the server with test input
echo "Running MCP Server with test messages..."
timeout 10s dotnet run < "$TEST_DIR/all_tests.json" > "$TEST_DIR/output.json" 2> "$TEST_DIR/stderr.log" || true

echo "Test Results:"
echo "=============="

# Check if we got responses
if [ -s "$TEST_DIR/output.json" ]; then
    echo "✓ Server responded to requests"
    echo ""
    echo "Server Output:"
    cat "$TEST_DIR/output.json" | jq . 2>/dev/null || cat "$TEST_DIR/output.json"
else
    echo "✗ No response from server"
fi

echo ""
echo "Server Logs:"
cat "$TEST_DIR/stderr.log"

# Cleanup
rm -rf "$TEST_DIR"

echo ""
echo "Test completed!"