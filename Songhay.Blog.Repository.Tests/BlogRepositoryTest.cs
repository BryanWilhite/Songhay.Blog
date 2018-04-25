using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Songhay.Blog.Models;
using Songhay.Blog.Repository.Tests.Extensions;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Songhay.Blog.Repository.Tests
{
    [TestClass]
    public class BlogRepositoryTest
    {
        static BlogRepositoryTest() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        static readonly TraceSource traceSource;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var basePath = FrameworkFileUtility.FindParentDirectory(Directory.GetCurrentDirectory(), this.GetType().Namespace, 5);
            cloudStorageAccount = this.TestContext.ShouldGetCloudStorageAccount(basePath);
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("inRoot", @"AzureBlobStorage-songhay\songhayblog-azurewebsites-net\BlogEntry\")]
        public async Task ShouldDownloadNewBlogEntries()
        {
            var projectDirectory = this.TestContext
                .ShouldGetAssemblyDirectoryInfo(this.GetType())
                ?.Parent
                ?.Parent
                ?.Parent.FullName;
            this.TestContext.ShouldFindDirectory(projectDirectory);

            #region test properties:

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var inRoot = this.TestContext.Properties["inRoot"].ToString();
            inRoot = Path.Combine(projectDirectory, inRoot);
            this.TestContext.ShouldFindDirectory(inRoot);

            #endregion

            var fileFilter = "*.json";
            var localSlugs = Directory.EnumerateFiles(inRoot, fileFilter)
                .Select(i => JsonConvert.DeserializeObject<BlogEntry>(File.ReadAllText(i)).Slug)
                .ToArray();

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);

            var newBlobs = new List<IListBlobItem>();

            var continuationToken = default(BlobContinuationToken);
            var prefix = "Entry";
            var blobListingDetails = BlobListingDetails.All;
            var maxResults = default(int?);
            var options = default(BlobRequestOptions);
            var operationContext = default(OperationContext);
            var useFlatBlobListing = true;

            do
            {
                var response = await container.ListBlobsSegmentedAsync(
                    prefix: prefix,
                    useFlatBlobListing: useFlatBlobListing,
                    blobListingDetails: blobListingDetails,
                    maxResults: maxResults,
                    currentToken: continuationToken,
                    options: options,
                    operationContext: operationContext);

                continuationToken = response.ContinuationToken;
                newBlobs.AddRange(response.Results);
            } while ((continuationToken != null));

            newBlobs.ToArray()
                .Where(i =>
                {
                    var name = (string)i.GetPropertyValue("Name");
                    if (string.IsNullOrEmpty(name)) return false;

                    name = name
                        .Replace("Entry/", string.Empty)
                        .Replace(".json", string.Empty);

                    return !localSlugs.Contains(name);
                })
                .ToArray();

            Assert.IsTrue(newBlobs.Any(), "The new blobs are not here.");
            Assert.IsTrue(newBlobs.Count() < localSlugs.Count(), "The expected new-Blob count is not here.");

            newBlobs.ForEachInEnumerable(async i =>
            {
                this.TestContext.WriteLine("Reading {0}...", i);
                var template = new
                {
                    Slug = string.Empty,
                    Title = string.Empty,
                    Author = string.Empty,
                    DateCreated = DateTime.Now,
                    Markdown = string.Empty,
                    IsPublished = false,
                    IsCodePrettified = false
                };

                var s = await (i as CloudBlockBlob).DownloadTextAsync();
                try
                {
                    var anon = JsonConvert.DeserializeAnonymousType(s, template);

                    var blogEntry = new BlogEntry
                    {
                        Author = anon.Author,
                        Content = anon.Markdown,
                        InceptDate = anon.DateCreated,
                        IsPublished = anon.IsPublished,
                        Slug = anon.Slug,
                        Title = anon.Title
                    };

                    var json = JsonConvert.SerializeObject(blogEntry, Formatting.Indented);
                    var path = string.Format("{0}{1}.json", inRoot, blogEntry.Slug);
                    File.WriteAllText(path, json);
                }
                catch (Exception ex)
                {
                    this.TestContext.WriteLine("File not well formed: {0}", i);
                    this.TestContext.WriteLine("Exception: {0}", ex.Message);
                }
            });
        }

        static CloudStorageAccount cloudStorageAccount;
    }
}
