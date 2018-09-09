using Microsoft.Extensions.Configuration;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Blog.Repository;
using Songhay.Blog.Repository.Extensions;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Songhay.Blog.Shell.Tests
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
            var projectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            cloudStorageAccount = (new ConfigurationBuilder()).ToCloudStorageAccount(projectInfo.FullName);
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("inRoot", @"AzureBlobStorage-songhay\songhayblog-azurewebsites-net\BlogEntry\")]
        public async Task ShouldDownloadNewBlogEntries()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var inRoot = this.TestContext.Properties["inRoot"].ToString();
            inRoot = projectDirectoryInfo.FullName.ToCombinedPath(inRoot);
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

                    var json = JsonConvert.SerializeObject(blogEntry, Newtonsoft.Json.Formatting.Indented);
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

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("entryHeaderElement", "h2")]
        [TestProperty("entryPath", @"html\ShouldGenerateBlogEntry.html")]
        [TestProperty("entryOutputPath", @"json\ShouldGenerateBlogEntry.json")]
        public async Task ShouldGenerateBlogEntry()
        {
            var projectInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());
            var webProjectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var entryHeaderElement = this.TestContext.Properties["entryHeaderElement"].ToString();

            var entryPath = this.TestContext.Properties["entryPath"].ToString();
            entryPath = projectInfo.FullName.ToCombinedPath(entryPath);
            this.TestContext.ShouldFindFile(entryPath);

            var entryOutputPath = this.TestContext.Properties["entryOutputPath"].ToString();
            entryOutputPath = projectInfo.FullName.ToCombinedPath(entryOutputPath);
            this.TestContext.ShouldFindFile(entryOutputPath);

            #endregion

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);

            var entry = File.ReadAllText(entryPath);
            entry = HtmlUtility.ConvertToXml(entry);
            var xdEntry = XDocument.Parse(entry);
            var body = xdEntry.Root.Element("body");
            Assert.IsNotNull(body, "The expected body is not here.");

            var title = body.Element(entryHeaderElement).GetInnerXml();
            var content = new XElement("body",
                body
                    .Elements()
                    .Where(i => !i.Name.LocalName.Equals(entryHeaderElement))
                ).GetInnerXml();

            var blogEntry = (new BlogEntry
            {
                Content = content,
                InceptDate = DateTime.Now,
                IsPublished = true,
                Title = title
            }).WithDefaultSlug();

            if (await repository.HasEntityAsync<BlogEntry>(blogEntry.Slug))
            {
                var previousEntry = await repository.LoadSingleAsync<BlogEntry>(blogEntry.Slug);
                Assert.IsNotNull(previousEntry, "The expected previous entry is not here.");
                blogEntry.InceptDate = previousEntry.InceptDate;
            }

            await repository.SaveEntityAsync(blogEntry);
            Assert.IsTrue(await repository.HasEntityAsync<BlogEntry>(blogEntry.Slug), "The expected Blog Entry is not in the Repository.");
            var json = blogEntry.ToJson(useJavaScriptCase: true);
            File.WriteAllText(entryOutputPath, json);
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("rssPath", @"ClientApp\src\assets\data\site-rss.xml")]
        public async Task ShouldGenerateBlogRss()
        {
            var webProjectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();

            var rssPath = this.TestContext.Properties["rssPath"].ToString();
            rssPath = webProjectInfo.FullName.ToCombinedPath(rssPath);
            this.TestContext.ShouldFindFile(rssPath);

            #endregion

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);
            var data = await (repository as IBlogEntryIndex).GetIndexAsync();
            Assert.IsTrue(data.Any(), "The expected data are not here.");

            var feed = data
                .OrderByDescending(i => i.InceptDate)
                .Take(10);

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Async = true,
                CloseOutput = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true
            };
            var person = new SyndicationPerson("Bryan Wilhite", "rasx@songhaysystem.com");

            using (var writer = XmlWriter.Create(builder, settings))
            {
                var feedWriter = new RssFeedWriter(writer);

                await feedWriter.WritePubDate(DateTime.Now);
                await feedWriter.WriteTitle($">DayPath_");
                await feedWriter.WriteDescription($"The technical journey of @BryanWilhite.");
                await feedWriter.WriteCopyright($"Bryan Wilhite, Songhay System {DateTime.Now.Year}");
                await feedWriter.Write(new SyndicationLink(new Uri("http://songhayblog.azurewebsites.net", UriKind.Absolute)));

                var tasks = feed.Select(async entry =>
                {
                    var item = new SyndicationItem
                    {
                        Description = entry.Content,
                        Id = entry.Slug,
                        LastUpdated = entry.ModificationDate,
                        Published = entry.InceptDate,
                        Title = entry.Title
                    };

                    item.AddContributor(person);
                    item.AddLink(new SyndicationLink(new Uri($"http://songhayblog.azurewebsites.net/blog/entry/{entry.Slug}", UriKind.Absolute)));

                    await feedWriter.Write(item);
                });

                await Task.WhenAll(tasks);
                await writer.FlushAsync();

            }

            File.WriteAllText(rssPath, builder.ToString());
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("indexPath", @"json\index.json")]
        [TestProperty("topicsPath", @"opml\topics.opml")]
        public async Task ShouldGenerateRepositoryIndex()
        {
            var projectInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();

            var indexPath = this.TestContext.Properties["indexPath"].ToString();
            indexPath = projectInfo.FullName.ToCombinedPath(indexPath);
            this.TestContext.ShouldFindFile(indexPath);

            var topicsPath = this.TestContext.Properties["topicsPath"].ToString();
            topicsPath = projectInfo.FullName.ToCombinedPath(topicsPath);
            this.TestContext.ShouldFindFile(topicsPath);

            #endregion

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);

            var json = await repository.ToRepositoryIndexAsync(topicsPath, useJavaScriptCase: true);

            await repository.SetIndex(json);

            File.WriteAllText(indexPath, json);

        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("blobContainerName", "songhayblog-azurewebsites-net")]
        [TestProperty("htmlFile", @"html\ShouldGenerateBlogEntry.html")]
        [TestProperty("slug", "yes-finally-here-architecting-ng-apps-with-redux-rxjs-and-nhrx-and-other-tweeted-links")]
        public async Task ShouldLoadEntryIntoHtmlFile()
        {
            var projectInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            var blobContainerName = this.TestContext.Properties["blobContainerName"].ToString();
            var htmlFile = this.TestContext.Properties["htmlFile"].ToString();
            htmlFile = Path.Combine(projectInfo.FullName, htmlFile);
            var slug = this.TestContext.Properties["slug"].ToString();

            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(blobContainerName);
            var keys = new AzureBlobKeys();
            keys.Add<BlogEntry>(i => i.Slug);

            var repository = new BlogRepository(keys, container);
            var blogEntry = await repository.LoadSingleAsync<BlogEntry>(slug);

            var h2 = new XElement("h2", blogEntry.Title);
            var contentHtml = string.Format("<content>{0}</content>", blogEntry.Content);
            contentHtml = HtmlUtility.ConvertToXml(contentHtml);
            var content = XElement.Parse(contentHtml);

            var html = File.ReadAllText(htmlFile);
            var xDoc = XDocument.Parse(html);
            var body = xDoc.Root.Element("body");
            body.Value = string.Empty;
            body.Add(h2);
            body.Add(content.Elements());

            File.WriteAllText(htmlFile, xDoc.ToString());
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("markdownPath", @"E:\~shares\sourceRoot\Git\Blog\2016-12\Working in markdown, leaving behind typing a typeface.md")]
        [TestProperty("entryPath", @"html\ShouldGenerateBlogEntry.html")]
        public void ShouldLoadEntryIntoHtmlFileFromMarkdown()
        {
            var projectRoot = this.TestContext.ShouldGetAssemblyDirectoryParent(this.GetType(), expectedLevels: 2);

            #region test properties:

            var markdownPath = this.TestContext.Properties["markdownPath"].ToString();
            this.TestContext.ShouldFindFile(markdownPath);

            var entryPath = this.TestContext.Properties["entryPath"].ToString();
            entryPath = projectRoot.ToCombinedPath(entryPath);
            this.TestContext.ShouldFindFile(entryPath);

            #endregion

            var markdown = File.ReadAllText(markdownPath);
            var entry = CommonMark.CommonMarkConverter.Convert(markdown);
            var contentHtml = string.Format("<content>{0}</content>", entry);
            var content = XElement.Parse(contentHtml);
            content.Element("h1").Name = "h2";

            var html = File.ReadAllText(entryPath);
            var xDoc = XDocument.Parse(html);
            var body = xDoc.Root.Element("body");
            body.Value = string.Empty;
            body.Add(content.Elements());

            File.WriteAllText(entryPath, xDoc.ToString());
        }

        static CloudStorageAccount cloudStorageAccount;
    }
}
