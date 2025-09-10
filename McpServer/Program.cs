using System.Text.Json;
using System.Text.Json.Nodes;

var server = new McpServer();
await server.RunAsync();

public class McpServer
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task RunAsync()
    {
        // Read from stdin and write to stdout for MCP protocol
        using var stdin = Console.OpenStandardInput();
        using var stdout = Console.OpenStandardOutput();
        using var reader = new StreamReader(stdin);
        using var writer = new StreamWriter(stdout) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var request = JsonNode.Parse(line);
                var response = HandleRequest(request);
                if (response != null)
                {
                    await writer.WriteLineAsync(response.ToJsonString(_jsonOptions));
                }
            }
            catch (Exception ex)
            {
                // Send error response
                var errorResponse = CreateErrorResponse(null, -32603, "Internal error", ex.Message);
                await writer.WriteLineAsync(errorResponse.ToJsonString(_jsonOptions));
            }
        }
    }

    private JsonNode? HandleRequest(JsonNode? request)
    {
        if (request == null) return CreateErrorResponse(null, -32700, "Parse error", null);

        var method = request["method"]?.ToString();
        var id = request["id"];

        return method switch
        {
            "initialize" => HandleInitialize(id),
            "initialized" => null, // Notification, no response needed
            "tools/list" => HandleToolsList(id),
            "tools/call" => HandleToolsCall(id, request["params"]),
            _ => CreateErrorResponse(id, -32601, "Method not found", null)
        };
    }

    private JsonNode HandleInitialize(JsonNode? id)
    {
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = new JsonObject
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new JsonObject
                {
                    ["tools"] = new JsonObject()
                },
                ["serverInfo"] = new JsonObject
                {
                    ["name"] = "PowerApps MCP Server",
                    ["version"] = "1.0.0"
                }
            }
        };
    }

    private JsonNode HandleToolsList(JsonNode? id)
    {
        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["result"] = new JsonObject
            {
                ["tools"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["name"] = "hello",
                        ["description"] = "Returns a hello message",
                        ["inputSchema"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["name"] = new JsonObject
                                {
                                    ["type"] = "string",
                                    ["description"] = "Name to greet"
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private JsonNode HandleToolsCall(JsonNode? id, JsonNode? parameters)
    {
        var toolName = parameters?["name"]?.ToString();
        
        if (toolName == "hello")
        {
            var name = parameters?["arguments"]?["name"]?.ToString() ?? "World";
            var message = $"Hello, {name}! This is a greeting from the PowerApps MCP Server.";
            
            return new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id?.DeepClone(),
                ["result"] = new JsonObject
                {
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = message
                        }
                    }
                }
            };
        }

        return CreateErrorResponse(id, -32602, "Invalid params", $"Unknown tool: {toolName}");
    }

    private JsonNode CreateErrorResponse(JsonNode? id, int code, string message, string? data)
    {
        var error = new JsonObject
        {
            ["code"] = code,
            ["message"] = message
        };
        
        if (data != null)
        {
            error["data"] = data;
        }

        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id?.DeepClone(),
            ["error"] = error
        };
    }
}
