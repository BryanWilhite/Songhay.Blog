using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Blog.Repository;
using Songhay.Cloud.BlobStorage.Extensions;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.Linq;

namespace Songhay.Blog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var meta = new ProgramMetadata();
            this.Configuration.Bind(nameof(ProgramMetadata), meta);

            var validation = meta.ToValidationResults();
            if (validation.Any()) throw new ApplicationException(validation.ToDisplayText());

            #region functional members:

            BlogRepository serveRepository(IServiceProvider factory)
            {
                var set = meta
                    .CloudStorageSet
                    .TryGetValueWithKey("SonghayCloudStorage");

                var connectionString = set.TryGetValueWithKey("classic");
                var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

                var repositoryKeys = new AzureBlobKeys();
                repositoryKeys.Add<BlogEntry>(e => e.Slug);

                var container = cloudStorageAccount.GetContainerReference(set.TryGetValueWithKey("classic-day-path-container"));

                return new BlogRepository(repositoryKeys, container);
            }

            #endregion

            services
                .AddSingleton<IRepositoryAsync, BlogRepository>(serveRepository)
                .AddSingleton(factory => meta.ToAzureSearchRestApiMetadata())
                .AddSingleton(factory => this.Configuration.GetSection(nameof(AzureSearchPostTemplate)).Get<AzureSearchPostTemplate>())
                .AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/dist";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
            }

            app.UseSpaStaticFiles();
            app
                .UseMvc()
                .UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501

                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
        }
    }
}
