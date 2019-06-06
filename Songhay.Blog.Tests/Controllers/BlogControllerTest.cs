using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Songhay.Blog.Models;
using Songhay.Extensions;
using Songhay.Models;
using System.IO;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Songhay.Blog.Tests.Controllers
{
    [TestClass]
    public class BlogControllerTest
    {
        [TestInitialize]
        public void InitializeTest()
        {

            var targetDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var basePath = targetDirectoryInfo.FullName;

            var builder = Program.CreateWebHostBuilder(args: null, builderAction: (builderContext, configBuilder) =>
            {
                Assert.IsNotNull(builderContext, "The expected Web Host builder context is not here.");

                this.TestContext.WriteLine($"configuring {nameof(TestServer)} with {nameof(basePath)}: {basePath}...");


                var env = builderContext.HostingEnvironment;
                Assert.IsNotNull(env, "The expected Hosting Environment is not here.");

                env.ContentRootPath = basePath;
                env.EnvironmentName = "Development";
                env.WebRootPath = $"{basePath}{Path.DirectorySeparatorChar}wwwroot";

                configBuilder.SetBasePath(env.ContentRootPath);
            });

            Assert.IsNotNull(builder, "The expected Web Host builder is not here.");

            this._server = new TestServer(builder);

            this.TestContext.WriteLine("initializing domain metadata...");
            this._meta = new ProgramMetadata();
            var configuration = this.TestContext.ShouldLoadConfigurationFromConventionalProject(this.GetType(), b =>
            {
                b.AddJsonFile(AppScalars.conventionalSettingsFile, optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), this._meta);
            this.TestContext.WriteLine($"{this._meta}");
        }

        public TestContext TestContext { get; set; }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("pathTemplate", "entry/{id}")]
        [TestProperty("id", "asp-net-web-api-ready-state-4-2017")]
        [TestProperty("outputFile", @"json\ShouldGetBlogEntryAsync.json")]
        public async Task ShouldGetBlogEntryAsync()
        {
            var projectInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var pathTemplate = new UriTemplate(string.Concat(baseRoute, this.TestContext.Properties["pathTemplate"].ToString()));
            var id = this.TestContext.Properties["id"].ToString();
            var outputFile = this.TestContext.Properties["outputFile"].ToString();
            outputFile = projectInfo.ToCombinedPath(outputFile);
            this.TestContext.ShouldFindFile(outputFile);

            #endregion

            var path = pathTemplate.BindByPosition(id);
            var client = this._server.CreateClient();
            var response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrEmpty(content), "The expected content is not here.");
            var jO = JObject.Parse(content);
            File.WriteAllText(outputFile, jO.ToString());
        }

        const string baseRoute = "api/blog/";

        ProgramMetadata _meta;
        TestServer _server;
    }
}
