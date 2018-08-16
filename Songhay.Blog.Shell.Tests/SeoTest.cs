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
        [TestProperty("defaultNamespace", "http://www.sitemaps.org/schemas/sitemap/0.9")]
        [TestProperty("indexJsonFile", @"json\index.json")]
        [TestProperty("rootUri", "http://songhayblog.azurewebsites.net")]
        [TestProperty("sitemapFile", @"ClientApp\src\sitemap.xml")]
        [TestProperty("urlTemplate", "/blog/entry/{slug}")]
        public void ShouldGenerateSitemap()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());
            var webProjectInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var defaultNamespace = XNamespace.Get(this.TestContext.Properties["defaultNamespace"].ToString());

            var indexJsonFile = this.TestContext.Properties["indexJsonFile"].ToString();
            indexJsonFile = Path.Combine(projectDirectoryInfo.FullName, indexJsonFile);
            this.TestContext.ShouldFindFile(indexJsonFile);

            var rootUri = new Uri(this.TestContext.Properties["rootUri"].ToString(), UriKind.Absolute);

            var sitemapFile = this.TestContext.Properties["sitemapFile"].ToString();
            sitemapFile = Path.Combine(webProjectInfo.FullName, sitemapFile);
            this.TestContext.ShouldFindFile(sitemapFile);

            var urlTemplate = new UriTemplate(this.TestContext.Properties["urlTemplate"].ToString());

            #endregion

            var indexEntries = JArray.Parse(File.ReadAllText(indexJsonFile));
            Assert.IsTrue(indexEntries.Any(), "The expected index entries are not here.");

            var sitemap = XDocument.Parse(File.ReadAllText(sitemapFile));

            if (sitemap.Root.HasElements) sitemap.Root.Elements().Remove();
            indexEntries.ForEachInEnumerable(i =>
            {
                var categoryJObject = JObject.Parse(string.Concat("{", i["itemCategory"].Value<string>(), "}"));

                var loc = urlTemplate.BindByPosition(rootUri, i["slug"].Value<string>());
                var lastmod = string.Format("{0}-{1}-{2}",
                    categoryJObject["year"].Value<string>(),
                    categoryJObject["month"].Value<string>(),
                    categoryJObject["day"].Value<string>());

                var url = new XElement(defaultNamespace + "url",
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
