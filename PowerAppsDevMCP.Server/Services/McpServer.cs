using System.Text.Json;
using Microsoft.Extensions.Logging;
using PowerAppsDevMCP.Server.Models;

namespace PowerAppsDevMCP.Server.Services;

public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer(ILogger<McpServer> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting PowerApps Dev MCP Server...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    _logger.LogInformation("No more input, shutting down.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                await ProcessMessage(line);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP server main loop");
        }
    }

    private async Task ProcessMessage(string messageJson)
    {
        try
        {
            var request = JsonSerializer.Deserialize<JsonRpcRequest>(messageJson, _jsonOptions);
            if (request == null)
            {
                _logger.LogWarning("Failed to deserialize request: {Message}", messageJson);
                return;
            }

            var response = await HandleRequest(request);
            if (response != null)
            {
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                await Console.Out.WriteLineAsync(responseJson);
                await Console.Out.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", messageJson);
            
            // Send error response
            var errorResponse = new JsonRpcResponse
            {
                Id = null,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
            
            var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
            await Console.Out.WriteLineAsync(errorJson);
            await Console.Out.FlushAsync();
        }
    }

    private async Task<JsonRpcResponse?> HandleRequest(JsonRpcRequest request)
    {
        return request.Method switch
        {
            "initialize" => await HandleInitialize(request),
            "initialized" => await HandleInitialized(request),
            "tools/list" => await HandleToolsList(request),
            "tools/call" => await HandleToolsCall(request),
            _ => CreateErrorResponse(request.Id, -32601, "Method not found")
        };
    }

    private Task<JsonRpcResponse> HandleInitialize(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling initialize request");

        var initializeParams = JsonSerializer.Deserialize<InitializeParams>(
            JsonSerializer.Serialize(request.Params), _jsonOptions);

        var result = new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability
                {
                    ListChanged = false
                }
            },
            ServerInfo = new ServerInfo
            {
                Name = "PowerApps Dev MCP Server",
                Version = "1.0.0"
            }
        };

        _logger.LogInformation("Server initialized with protocol version {Version}", result.ProtocolVersion);

        return Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = result
        });
    }

    private Task<JsonRpcResponse?> HandleInitialized(JsonRpcRequest request)
    {
        _logger.LogInformation("Client has completed initialization");
        return Task.FromResult<JsonRpcResponse?>(null); // No response needed for notification
    }

    private Task<JsonRpcResponse> HandleToolsList(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling tools/list request");

        var tools = new[]
        {
            new Tool
            {
                Name = "hello",
                Description = "Returns a hello world greeting",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new
                        {
                            type = "string",
                            description = "Optional name to include in greeting"
                        }
                    }
                }
            }
        };

        var result = new ToolsListResult
        {
            Tools = tools
        };

        return Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = result
        });
    }

    private async Task<JsonRpcResponse> HandleToolsCall(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling tools/call request");

        var callParams = JsonSerializer.Deserialize<CallToolParams>(
            JsonSerializer.Serialize(request.Params), _jsonOptions);

        if (callParams == null)
        {
            return CreateErrorResponse(request.Id, -32602, "Invalid params");
        }

        return callParams.Name switch
        {
            "hello" => await HandleHelloTool(request.Id, callParams),
            _ => CreateErrorResponse(request.Id, -32602, $"Unknown tool: {callParams.Name}")
        };
    }

    private Task<JsonRpcResponse> HandleHelloTool(object? requestId, CallToolParams callParams)
    {
        var name = callParams.Arguments?.GetValueOrDefault("name")?.ToString() ?? "World";
        var greeting = $"Hello, {name}! This is a greeting from the PowerApps Dev MCP Server.";

        _logger.LogInformation("Generated greeting: {Greeting}", greeting);

        var result = new CallToolResult
        {
            Content = new[]
            {
                new ToolContent
                {
                    Type = "text",
                    Text = greeting
                }
            }
        };

        return Task.FromResult(new JsonRpcResponse
        {
            Id = requestId,
            Result = result
        });
    }

    private JsonRpcResponse CreateErrorResponse(object? id, int code, string message)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message
            }
        };
    }
}