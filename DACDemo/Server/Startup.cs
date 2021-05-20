using System;
using System.Net.Http.Headers;
using Azure.Core;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Azure.Core.Diagnostics;

namespace DACDemo.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var rc = new TokenRequestContext(new string[] { "https://graph.microsoft.com/.default" });
            AzureEventSourceListener.CreateConsoleLogger();
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Configure the credential options
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true,
                Diagnostics = { IsLoggingEnabled = true }
            };

            // Create a DefaultAzureCredential
            var cred = new DefaultAzureCredential(options);

            // Initialize a BlobContainerClient with the credential.
            services.AddSingleton(new BlobContainerClient(new Uri(Configuration["BlobUri"]), cred));

            // Initialize a Graph client with the credential.
            GraphServiceClient graphServiceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", cred.GetToken(rc).Token);

                    return Task.CompletedTask;
                }));
            services.AddSingleton(graphServiceClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
