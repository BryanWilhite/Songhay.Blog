using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Songhay.Blog.Models;
using Songhay.Blog.Repository.Extensions;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Songhay.Blog.Repository.Tests
{
    [TestClass]
    public class BlogRepositoryTest
    {
        static BlogRepositoryTest() => traceSource = TraceSources
            .Instance
            .GetConfiguredTraceSource()
            .WithSourceLevels();

        static readonly TraceSource traceSource;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var projectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var builder = new ConfigurationBuilder();

            cloudStorageAccountClassic = builder.ToCloudStorageAccount(projectInfo.FullName);
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        public async Task ShouldGetIndex()
        {

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();

            var container = cloudStorageAccountClassic.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);

            var sampleSize = 10;
            var index = await repository.GetIndexAsync();
            index = index
                .OrderByDescending(i => i.InceptDate)
                .Take(sampleSize);
            Assert.IsTrue(index.Any(), "The expected repository index is not here.");

            index.ForEachInEnumerable(i => this.TestContext.WriteLine(i.ToString()));

            Assert.AreEqual(sampleSize, index.Count(), "The expected repository index count is not here.");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("slug", "inter-view-model-communication")]
        public async Task ShouldHaveEntity()
        {
            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var slug = this.TestContext.Properties["slug"].ToString();

            var container = cloudStorageAccountClassic.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);
            Assert.IsTrue(await repository.HasEntityAsync<BlogEntry>(slug), "The expected Blog Entry is not in the Repository.");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("slugNew", "non-blocking-ui-s-with-interface-previews-themadray-and-other-twinks")]
        [TestProperty("slugOld", "non-blocking-ui-s-with-interface-previews-and-other-twinks")]
        public async Task ShouldReplaceEntry()
        {
            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var slugNew = this.TestContext.Properties["slugNew"].ToString();
            var slugOld = this.TestContext.Properties["slugOld"].ToString();

            var container = cloudStorageAccountClassic.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);
            var blogEntryNew = await repository.LoadSingleAsync<BlogEntry>(slugNew);
            var blogEntryOld = await repository.LoadSingleAsync<BlogEntry>(slugOld);

            blogEntryNew.InceptDate = blogEntryOld.InceptDate;
            blogEntryNew.ModificationDate = DateTime.Now;

            await repository.SaveEntityAsync(blogEntryNew);
            await repository.DeleteEntityAsync<BlogEntry>(blogEntryOld.Slug);
        }

        static CloudStorageAccount cloudStorageAccountClassic;
    }
}
