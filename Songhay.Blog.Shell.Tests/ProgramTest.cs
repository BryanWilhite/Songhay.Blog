using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Songhay.Blog.Models;
using Songhay.Blog.Repository.Extensions;
using Songhay.Cloud.BlobStorage.Extensions;
using Songhay.Extensions;
using Songhay.Models;
using System.IO;
using System.Threading.Tasks;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public class ProgramTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var projectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var builder = new ConfigurationBuilder();

            cloudStorageAccount = builder.ToCloudStorageAccount(projectInfo.FullName, AppScalars.cloudStorageAccountGeneralPurposeV1);
        }

        [TestMethod]
        [TestProperty("appFile", @"json\app.json")]
        [TestProperty("blobContainerName", "day-path-blog")]
        [TestProperty("indexFile", @"json\index.json")]
        [TestProperty("serverMetadataFile", @"json\server-meta.json")]
        public async Task ShouldGenerateAppData()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var appFile = this.TestContext.Properties["appFile"].ToString();
            appFile = projectDirectoryInfo.ToCombinedPath(appFile);
            this.TestContext.ShouldFindFile(appFile);

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();

            var indexFile = this.TestContext.Properties["indexFile"].ToString();
            indexFile = projectDirectoryInfo.ToCombinedPath(indexFile);
            this.TestContext.ShouldFindFile(indexFile);

            var serverMetadataFile = this.TestContext.Properties["serverMetadataFile"].ToString();
            serverMetadataFile = projectDirectoryInfo.ToCombinedPath(serverMetadataFile);
            this.TestContext.ShouldFindFile(serverMetadataFile);

            #endregion

            var serverMetaRoot = "serverMeta";
            var indexRoot = "index";

            var jO = JObject.Parse(File.ReadAllText(appFile));

            var jO_serverMetadata = JObject.Parse(File.ReadAllText(serverMetadataFile));

            var jA_index = JArray.Parse(File.ReadAllText(indexFile));

            jO[serverMetaRoot] = jO_serverMetadata;
            jO[indexRoot] = jA_index;

            File.WriteAllText(appFile, jO.ToString());

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            await container.UploadBlobAsync(appFile, string.Empty);
        }

        [TestMethod]
        [TestProperty("serverMetadataFile", @"json\server-meta.json")]
        public void ShouldGenerateServerMetadata()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var serverMetadataFile = this.TestContext.Properties["serverMetadataFile"].ToString();
            serverMetadataFile = Path.Combine(projectDirectoryInfo.FullName, serverMetadataFile);
            this.TestContext.ShouldFindFile(serverMetadataFile);

            #endregion

            var jO = JObject.Parse(File.ReadAllText(serverMetadataFile));
            var assemblyInfo = new FrameworkAssemblyInfo(typeof(Program).Assembly);

            this.TestContext.WriteLine($"assemblyInfo: {assemblyInfo}");

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            jO[nameof(assemblyInfo)] = JObject.FromObject(assemblyInfo, JsonSerializer.Create(settings));

            File.WriteAllText(serverMetadataFile, jO.ToString());
        }

        static CloudStorageAccount cloudStorageAccount;
    }
}
