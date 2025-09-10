using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging based on Microsoft's MCP approach
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr as per Microsoft example
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class PowerAppsTools
{
    [McpServerTool, Description("Returns a hello world message from the PowerApps MCP Server.")]
    public static string Hello(string message = "World") => $"Hello from PowerApps MCP Server: {message}";

    [McpServerTool, Description("Echoes the message back in reverse from the PowerApps MCP Server.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
