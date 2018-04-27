using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Songhay.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Tavis.UriTemplates;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public class SeoTest
    {
        public TestContext TestContext { get; set; }
        [TestMethod]
        [TestProperty("indexJsonFile", @"AzureBlobStorage-songhay\songhayblog-azurewebsites-net\index.json")]
        [TestProperty("rootUri", "http://songhayblog.azurewebsites.net")]
        [TestProperty("sitemapFile", @"Songhay.Blog\sitemap.xml")]
        [TestProperty("urlTemplate", "/entry/{slug}")]
        public void ShouldGenerateSitemap()
        {
            var projectsRoot = this.TestContext
                .ShouldGetAssemblyDirectoryInfo(this.GetType())
                ?.Parent
                ?.Parent
                ?.Parent.FullName;
            this.TestContext.ShouldFindDirectory(projectsRoot);

            #region test properties:

            var indexJsonFile = this.TestContext.Properties["indexJsonFile"].ToString();
            indexJsonFile = Path.Combine(projectsRoot, indexJsonFile);
            this.TestContext.ShouldFindFile(indexJsonFile);

            var rootUri = new Uri(this.TestContext.Properties["rootUri"].ToString(), UriKind.Absolute);

            var sitemapFile = this.TestContext.Properties["sitemapFile"].ToString();
            sitemapFile = Path.Combine(projectsRoot, sitemapFile);
            this.TestContext.ShouldFindFile(sitemapFile);

            var urlTemplate = new UriTemplate(this.TestContext.Properties["urlTemplate"].ToString());

            #endregion

            var indexEntries = JArray.Parse(File.ReadAllText(indexJsonFile));
            Assert.IsTrue(indexEntries.Any(), "The expected index entries are not here.");

            var sitemap = XDocument.Parse(File.ReadAllText(sitemapFile));

            if (sitemap.Root.HasElements) sitemap.Root.Elements().Remove();
            indexEntries.ForEachInEnumerable(i =>
            {
                var categoryJObject = JObject.Parse(string.Concat("{", i["ItemCategory"].Value<string>(), "}"));

                var loc = urlTemplate.BindByPosition(rootUri, i["Slug"].Value<string>());
                var lastmod = string.Format("{0}-{1}-{2}",
                    categoryJObject["year"].Value<string>(),
                    categoryJObject["month"].Value<string>(),
                    categoryJObject["day"].Value<string>());

                var url = new XElement("url",
                    new XElement("loc", loc),
                    new XElement("lastmod", lastmod),
                    new XElement("changefreq", "monthly")
                    );

                sitemap.Root.Add(url);
            });

            sitemap.Save(sitemapFile);
        }
    }
}
