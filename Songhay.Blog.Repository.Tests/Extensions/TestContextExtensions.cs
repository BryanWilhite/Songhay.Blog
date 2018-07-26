using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Models;
using Songhay.Xml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Songhay.Blog.Repository.Tests.Extensions
{
    public static class TestContextExtensions
    {
        public static CloudStorageAccount ShouldGetCloudStorageAccount(this TestContext context, string basePath)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(AppScalars.conventionalSettingsFile, optional: false, reloadOnChange: true);

            var meta = new ProgramMetadata();
            builder.Build().Bind(nameof(ProgramMetadata), meta);
            context.WriteLine($"{meta}");

            Assert.IsNotNull(meta.CloudStorageSet, "The expected cloud storage set is not here.");

            var key = "SonghayCloudStorage";
            var test = meta.CloudStorageSet.TryGetValue(key, out var set);
            Assert.IsTrue(test, $"The expected cloud storage set, {key}, is not here.");
            Assert.IsTrue(set.Any(), $"The expected cloud storage set items for {key} are not here.");

            var connectionString = set.First().Value;
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            return cloudStorageAccount;
        }

        public static async Task<string> ShouldGenerateRepositoryIndex(this TestContext context, BlogRepository repository, string topicsPath, bool useJavaScriptCase)
        {
            Assert.IsNotNull(repository, "The expected repository is not here.");

            var entries = await repository.LoadAllAsync<BlogEntry>();
            Assert.IsTrue(entries.Any(), "The expected Blog entries are not here.");

            var xd = XDocument.Load(topicsPath);
            var topics = OpmlUtility.GetDocument(xd.Root, OpmlUtility.rx);

            var json = entries.GenerateIndex(topics, useJavaScriptCase);
            Assert.IsFalse(string.IsNullOrEmpty(json), "The expected index data is not here.");

            return json;
        }
    }
}
