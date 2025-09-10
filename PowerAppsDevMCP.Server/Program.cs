using Microsoft.Extensions.Logging;
using PowerAppsDevMCP.Server.Services;

// Configure logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<McpServer>();

// Create and run the MCP server
var mcpServer = new McpServer(logger);

// Handle shutdown gracefully
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await mcpServer.RunAsync(cts.Token);
