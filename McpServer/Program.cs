using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.ComponentModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using Azure.Identity;
using System.Text.Json;

// Parse command line arguments
var environmentUrlOption = new Option<string?>(
    name: "--environment-url",
    description: "The Dataverse environment URL (e.g., https://yourorg.crm.dynamics.com)")
{
    IsRequired = false
};

var rootCommand = new RootCommand("PowerApps MCP Server")
{
    environmentUrlOption
};

string? environmentUrl = null;

rootCommand.SetHandler((url) =>
{
    environmentUrl = url;
}, environmentUrlOption);

// Parse the arguments but don't invoke the command
var parseResult = rootCommand.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (var error in parseResult.Errors)
    {
        Console.Error.WriteLine(error.Message);
    }
    Environment.Exit(1);
}

// Execute the handler to extract the environment URL
environmentUrl = parseResult.GetValueForOption(environmentUrlOption);

// Store the environment URL globally
PowerAppsTools.SetEnvironmentUrl(environmentUrl);

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
    private static string? _environmentUrl;

    public static void SetEnvironmentUrl(string? url)
    {
        _environmentUrl = url;
    }

    [McpServerTool, Description("Returns a hello world message from the PowerApps MCP Server.")]
    public static string Hello(string message = "World") => $"Hello from PowerApps MCP Server: {message}";

    [McpServerTool, Description("Echoes the message back in reverse from the PowerApps MCP Server.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());

    [McpServerTool, Description("Connects to Microsoft Dataverse and returns information about the authenticated user using WhoAmI request.")]
    public static async Task<string> WhoAmI()
    {
        try
        {
            var environmentUrl = _environmentUrl;
            
            // Validate the environment URL
            if (string.IsNullOrWhiteSpace(environmentUrl))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Environment URL not configured. Please specify --environment-url when starting the server.", 
                    success = false 
                });
            }

            // Ensure the URL is properly formatted
            if (!environmentUrl.StartsWith("https://"))
            {
                environmentUrl = "https://" + environmentUrl;
            }

            // Create DefaultAzureCredential for authentication
            var credential = new DefaultAzureCredential();

            // Create the Dataverse service client
            var serviceClient = new ServiceClient(
                instanceUrl: new Uri(environmentUrl),
                tokenProviderFunction: async (string resource) =>
                {
                    var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { $"{resource}/.default" }));
                    return token.Token;
                },
                useUniqueInstance: true,
                logger: null);

            // Check if the connection is successful
            if (!serviceClient.IsReady)
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = $"Failed to connect to Dataverse: {serviceClient.LastError}", 
                    success = false 
                });
            }

            // Execute WhoAmI request using the Execute method
            var whoAmIRequest = new WhoAmIRequest();
            var whoAmIResponse = (WhoAmIResponse)await serviceClient.ExecuteAsync(whoAmIRequest);

            // Return the response data
            var result = new
            {
                success = true,
                userId = whoAmIResponse.UserId,
                businessUnitId = whoAmIResponse.BusinessUnitId,
                organizationId = whoAmIResponse.OrganizationId,
                environmentUrl = environmentUrl,
                timestamp = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = ex.Message, 
                success = false,
                type = ex.GetType().Name,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
