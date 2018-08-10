using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Songhay.Extensions;
using Songhay.Models;
using System.IO;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public class ProgramTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestProperty("serverMetadataFile", @"ClientApp\src\assets\data\server-meta.json")]
        public void ShouldGenerateServerMetadata()
        {
            var webProjectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var serverMetadataFile = this.TestContext.Properties["serverMetadataFile"].ToString();
            serverMetadataFile = Path.Combine(webProjectInfo.FullName, serverMetadataFile);
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
    }
}
