using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace McpServer.Tests;

public class WhoAmITests
{
    private IMcpClient? _client;
    private StdioClientTransport? _transport;

    [SetUp]
    public async Task Setup()
    {
        // Get the correct path to the McpServer project from the test directory
        var testDirectory = TestContext.CurrentContext.TestDirectory;
        var solutionDirectory = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "..", ".."));
        var serverProjectPath = Path.Combine(solutionDirectory, "McpServer", "McpServer.csproj");
        
        // Verify the server project exists
        if (!File.Exists(serverProjectPath))
        {
            throw new FileNotFoundException($"McpServer project not found at: {serverProjectPath}");
        }

        // Create transport to connect to our McpServer using stdio
        // Pass environment URL as command line argument for security
        _transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "McpServer",
            Command = "dotnet",
            Arguments = ["run", "--project", serverProjectPath, "--", "--environment-url", "https://example.crm.dynamics.com"],
            WorkingDirectory = solutionDirectory
        });

        // Create client and connect to server
        _client = await McpClientFactory.CreateAsync(_transport);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
        
        _transport = null;
    }

    [Test]
    public async Task WhoAmI_WithConfiguredUrl_ShouldReturnConnectionError()
    {
        // Arrange - URL is now configured via command line argument in Setup
        var arguments = new Dictionary<string, object?>();

        // Act
        var result = await _client!.CallToolAsync("who_am_i", arguments);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");
        Assert.That(result.Content, Is.Not.Empty, "Result content should not be empty");
        
        var textContent = result.Content.FirstOrDefault(c => c.Type == "text") as TextContentBlock;
        Assert.That(textContent, Is.Not.Null, "Should have text content");
        
        var response = JsonSerializer.Deserialize<JsonElement>(textContent!.Text);
        Assert.That(response.GetProperty("success").GetBoolean(), Is.False, "Should return success=false due to connection error");
        Assert.That(response.TryGetProperty("error", out _), Is.True, "Should contain error property");
    }

    [Test]
    public async Task WhoAmI_WithEmptyUrl_ShouldReturnValidationError()
    {
        // This test needs a separate server instance with no environment URL
        var testDirectory = TestContext.CurrentContext.TestDirectory;
        var solutionDirectory = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "..", ".."));
        var serverProjectPath = Path.Combine(solutionDirectory, "McpServer", "McpServer.csproj");
        
        var emptyUrlTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "McpServerNoUrl",
            Command = "dotnet",
            Arguments = ["run", "--project", serverProjectPath],
            WorkingDirectory = solutionDirectory
        });

        var emptyUrlClient = await McpClientFactory.CreateAsync(emptyUrlTransport);

        try
        {
            // Arrange - No URL configured
            var arguments = new Dictionary<string, object?>();

            // Act
            var result = await emptyUrlClient.CallToolAsync("who_am_i", arguments);

            // Assert
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Content, Is.Not.Empty, "Result content should not be empty");
            
            var textContent = result.Content.FirstOrDefault(c => c.Type == "text") as TextContentBlock;
            Assert.That(textContent, Is.Not.Null, "Should have text content");
            
            var response = JsonSerializer.Deserialize<JsonElement>(textContent!.Text);
            Assert.That(response.GetProperty("success").GetBoolean(), Is.False, "Should return success=false for missing URL");
            Assert.That(response.GetProperty("error").GetString(), Does.Contain("Environment URL not configured"), "Should contain validation error message");
        }
        finally
        {
            await emptyUrlClient.DisposeAsync();
        }
    }

    [Test]
    public async Task WhoAmI_WithInvalidUrl_ShouldReturnConnectionError()
    {
        // This test needs a separate server instance with an invalid URL
        var testDirectory = TestContext.CurrentContext.TestDirectory;
        var solutionDirectory = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "..", ".."));
        var serverProjectPath = Path.Combine(solutionDirectory, "McpServer", "McpServer.csproj");
        
        var invalidUrlTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "McpServerInvalidUrl",
            Command = "dotnet",
            Arguments = ["run", "--project", serverProjectPath, "--", "--environment-url", "https://invalid-nonexistent-url.crm.dynamics.com"],
            WorkingDirectory = solutionDirectory
        });

        var invalidUrlClient = await McpClientFactory.CreateAsync(invalidUrlTransport);

        try
        {
            // Arrange - Invalid URL configured via command line
            var arguments = new Dictionary<string, object?>();

            // Act
            var result = await invalidUrlClient.CallToolAsync("who_am_i", arguments);

            // Assert
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Content, Is.Not.Empty, "Result content should not be empty");
            
            var textContent = result.Content.FirstOrDefault(c => c.Type == "text") as TextContentBlock;
            Assert.That(textContent, Is.Not.Null, "Should have text content");
            
            var response = JsonSerializer.Deserialize<JsonElement>(textContent!.Text);
            Assert.That(response.GetProperty("success").GetBoolean(), Is.False, "Should return success=false for invalid URL");
            Assert.That(response.TryGetProperty("error", out _), Is.True, "Should contain error property for connection failure");
        }
        finally
        {
            await invalidUrlClient.DisposeAsync();
        }
    }

    [Test]
    public async Task Server_ShouldHaveWhoAmITool()
    {
        // Act
        var tools = await _client!.ListToolsAsync();

        // Debug: Print all available tools
        TestContext.WriteLine($"Available tools count: {tools.Count}");
        foreach (var tool in tools)
        {
            TestContext.WriteLine($"Tool: {tool.Name} - {tool.Description}");
        }

        // Assert
        var whoAmITool = tools.FirstOrDefault(t => t.Name == "who_am_i");
        Assert.That(whoAmITool, Is.Not.Null, "Server should have who_am_i tool");
        Assert.That(whoAmITool!.Description, Does.Contain("Dataverse"), "WhoAmI tool should mention Dataverse in description");
    }
}