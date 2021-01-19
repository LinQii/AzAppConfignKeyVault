using System;
using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

namespace AzAppConfignKeyVault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureAppConfiguration((context, config) =>
                {
                    var settings = config.Build();

                    var appConfigurationEndPoint = settings["AppConfiguration:EndPoint"];

                    if (string.IsNullOrEmpty(appConfigurationEndPoint))
                    {
                        return;
                    }

                    TokenCredential credentials;
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                        if (string.IsNullOrEmpty(clientId))
                        {
                            return;
                        }

                        credentials = new DefaultAzureCredential();
                    }
                    else
                    {
                        // Use specific identity
                        var clientId = settings["AppConfiguration:ClientIdentityId"];

                        if (string.IsNullOrEmpty(clientId))
                        {
                            return;
                        }

                        credentials = new ManagedIdentityCredential(clientId);
                    }

                    var endpoint = new Uri(appConfigurationEndPoint);
                    config.AddAzureAppConfiguration(options =>
                    {
                        options
                            // Use managed identity to access app configuration
                            .Connect(endpoint, credentials)

                            // Configure Azure Key Vault with Managed Identity
                            .ConfigureKeyVault(vaultOpt => vaultOpt.SetCredential(credentials))

                            // Filter environment prefix and label
                            .Select(keyFilter: "*", labelFilter: context.HostingEnvironment.EnvironmentName)

                            // Trim prefix for multi app service
                            .TrimKeyPrefix(prefix: "*")

                            // Config refresh for Setting:*
                            .ConfigureRefresh((refreshOptions) =>
                            {
                                refreshOptions
                                    .Register(key: "ColorSettings:Sentinel", label: context.HostingEnvironment.EnvironmentName, refreshAll: true)
                                    .SetCacheExpiration(TimeSpan.FromSeconds(10));
                            })
                            .UseFeatureFlags();
                    });
                })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
