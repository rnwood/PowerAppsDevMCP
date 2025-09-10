using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace McpServer.Tests;

public class McpServerTests
{
    [Test]
    public void TestServerProjectExists()
    {
        // Verify the server project exists and can be built
        var serverProjectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer", "McpServer.csproj");
        Assert.That(File.Exists(serverProjectPath), Is.True, "McpServer project should exist");
    }

    [Test]
    public async Task TestServerBuild()
    {
        // Test that the server project builds successfully
        var serverProjectDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build",
                WorkingDirectory = serverProjectDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        Assert.That(process.ExitCode, Is.EqualTo(0), "Server project should build successfully");
    }

    [Test]
    public async Task TestServerStartsAndStops()
    {
        // Test that the server can start and responds to basic input
        var serverProjectDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run",
                WorkingDirectory = serverProjectDir,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        // Give the server time to start
        await Task.Delay(2000);

        // Send a simple initialization message
        var initMessage = """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{}}}""";
        await process.StandardInput.WriteLineAsync(initMessage);
        await process.StandardInput.FlushAsync();

        // Give time for processing
        await Task.Delay(1000);

        // Check that the process is still running (not crashed)
        Assert.That(process.HasExited, Is.False, "Server should still be running after receiving initialization");

        // Terminate the process
        if (!process.HasExited)
        {
            process.Kill();
            await process.WaitForExitAsync();
        }

        process.Dispose();
    }

    [Test]
    public void TestNoDotNet8CompatibilityWarnings()
    {
        // Test that there are no .NET 8 compatibility warnings in the project file
        var projectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer", "McpServer.csproj");
        var projectContent = File.ReadAllText(projectPath);
        
        // Should not contain the problematic package
        Assert.That(projectContent, Does.Not.Contain("Microsoft.CrmSdk.CoreAssemblies"), 
            "Project should not contain Microsoft.CrmSdk.CoreAssemblies which is not compatible with .NET 8");
    }

    [Test]
    public void TestRequiredPackagesPresent()
    {
        // Test that required packages are present
        var projectPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer", "McpServer.csproj");
        var projectContent = File.ReadAllText(projectPath);
        
        Assert.That(projectContent, Does.Contain("Microsoft.PowerPlatform.Dataverse.Client"), 
            "Project should contain Microsoft.PowerPlatform.Dataverse.Client package");
        Assert.That(projectContent, Does.Contain("Azure.Identity"), 
            "Project should contain Azure.Identity package");
        Assert.That(projectContent, Does.Contain("ModelContextProtocol"), 
            "Project should contain ModelContextProtocol package");
    }

    [Test]
    public void TestWhoAmIFunctionExists()
    {
        // Test that the WhoAmI function exists in the code
        var programPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer", "Program.cs");
        var programContent = File.ReadAllText(programPath);
        
        Assert.That(programContent, Does.Contain("WhoAmI"), 
            "Program should contain WhoAmI function");
        Assert.That(programContent, Does.Contain("environmentUrl"), 
            "WhoAmI function should accept environmentUrl parameter");
    }

    [Test]
    public void TestUsesDotNetCompatibleSDK()
    {
        // Test that the code uses .NET 8 compatible Dataverse SDK approaches
        var programPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "McpServer", "Program.cs");
        var programContent = File.ReadAllText(programPath);
        
        Assert.That(programContent, Does.Contain("DefaultAzureCredential"), 
            "Should use DefaultAzureCredential for authentication");
        Assert.That(programContent, Does.Contain("Microsoft.PowerPlatform.Dataverse.Client"), 
            "Should use the official Dataverse client");
    }
}