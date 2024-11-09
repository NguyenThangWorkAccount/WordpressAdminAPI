using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WordpressAdminApi
{
    public class Program
    {
        private const int TimeoutMinutes = 10;
        private const int PollIntervalSeconds = 5;

        public static Task Main(string[] args)
        {
            // Check if starting or stopping
            if (args.Contains("-stop"))
            {
                HandleStopCommand(args).Wait();
                return Task.CompletedTask;
            }
            else if (args.Contains("-start"))
            {
                HandleStartCommand(args).Wait();
                return Task.CompletedTask;
            }

            Console.WriteLine("Invalid command. Use -start or -stop.");
            return Task.CompletedTask;
        }

        private static Task HandleStopCommand(string[] args)
        {
            var portIndex = Array.IndexOf(args, "-port");
            string port = portIndex >= 0 && args.Length > portIndex + 1 ? args[portIndex + 1] : "5000";

            Console.WriteLine($"Attempting to stop server on port: {port}");

            // Stop the server process running on the given port
            bool stopped = StopServer(port);
            Console.WriteLine(stopped
                ? $"Server on port {port} stopped successfully."
                : $"Failed to stop server on port {port}. Make sure it is running and accessible.");

            return Task.CompletedTask;
        }

        private static async Task HandleStartCommand(string[] args)
        {
            // Extract command-line arguments or use default settings
            var port = GetArgument(args, "-port", "4500");
            var apiKeyPath = GetArgument(args, "-apiKeyPath", "");
            var sheetId = GetArgument(args, "-sheetId", "");

            Console.WriteLine($"Starting server on port {port} with API Key Path: {apiKeyPath} and Sheet ID: {sheetId}");

            if (IsPortInUse(port))
            {
                Console.WriteLine($"Port {port} is in use. Checking if server is fully ready...");

                var isServerReady = await WaitForServerReadiness(port);
                if (!isServerReady)
                {
                    Console.WriteLine($"Server on port {port} did not become ready within {TimeoutMinutes} minutes. Exiting.");
                    return;
                }

                Console.WriteLine($"Server on port {port} is ready to handle requests.");
                return; // Exit if the server is already running and ready
            }

            // Start the host
            var host = CreateHostBuilder(args, port, apiKeyPath, sheetId).Build();
            host.Run();
        }

        private static string GetArgument(string[] args, string name, string defaultValue)
        {
            var index = Array.IndexOf(args, name);
            return index >= 0 && args.Length > index + 1 ? args[index + 1] : defaultValue;
        }

        private static bool IsPortInUse(string port)
        {
            int portNumber = int.Parse(port);

            try
            {
                using var client = new TcpClient("localhost", portNumber);
                return true;
            }
            catch (SocketException)
            {
                return false; // Port is not in use
            }
        }

        private static async Task<bool> WaitForServerReadiness(string port)
        {
            using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < TimeSpan.FromMinutes(TimeoutMinutes))
            {
                try
                {
                    // Send a basic request to check if the server is responding
                    var response = await httpClient.GetAsync($"http://localhost:{port}/"); // Checking the root endpoint
                    if (response.IsSuccessStatusCode)
                    {
                        return true; // Server is ready
                    }
                }
                catch
                {
                    // Ignore exceptions, as they indicate the server isn't ready yet
                }

                // Wait for a few seconds before retrying
                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds));
            }

            return false; // Timeout reached, server is not ready
        }

        private static bool StopServer(string port)
        {
            try
            {
                // Get the PID of the process using the port
                var pid = GetProcessIdByPort(port);

                if (pid == -1)
                {
                    Console.WriteLine($"No server found on port {port}.");
                    return false;
                }

                // Kill the process with the given PID
                Process.GetProcessById(pid).Kill();
                Console.WriteLine($"Server on port {port} stopped.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping server: {ex.Message}");
                return false;
            }
        }

        private static int GetProcessIdByPort(string port)
        {
            int pid = -1;
            string command = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ?
                $"netstat -ano | findstr :{port}" : $"lsof -t -i:{port}"; // Windows: netstat, Linux/macOS: lsof

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    Console.WriteLine("Failed to start the process.");
                    return -1; // Exit the method or handle the failure appropriately
                }

                using (var reader = process.StandardOutput)
                {
                    var output = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(output))
                    {
                        var outputParts = output.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (outputParts.Any())
                        {
                            pid = int.Parse(outputParts.Last());
                        }
                    }
                }
            }

            return pid;
        }

        private static IHostBuilder CreateHostBuilder(string[] args, string port, string apiKeyPath, string sheetId) 
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://localhost:{port}"); 
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(int.Parse(port));
                    });

                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("AppConfig:Port", port),
                            new KeyValuePair<string, string>("GoogleSheetsConfig:ApiKeyPath", apiKeyPath),
                            new KeyValuePair<string, string>("GoogleSheetsConfig:SheetId", sheetId),
                        })
                        .Build();
                    services.AddSingleton<IConfiguration>(config);
                });
        }
    }
}
