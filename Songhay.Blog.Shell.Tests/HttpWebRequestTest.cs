using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Xml;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tavis.UriTemplates;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public class HttpWebRequestTest
    {
        static HttpWebRequestTest()
        {
            httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false, // this is set false for ShouldGet301Or302()
            });
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var targetDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var basePath = targetDirectoryInfo.FullName;
            var meta = new ProgramMetadata();
            var configuration = this.TestContext.ShouldLoadConfigurationFromConventionalProject(this.GetType(), b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), meta);
            this.TestContext.WriteLine($"{meta}");

            restApiMetadata = meta.ToAzureSearchRestApiMetadata();

        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        public async Task ShouldDeleteAzureSearchServiceComponent()
        {
            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search"]);
            var componentName = restApiMetadata.ClaimsSet["search-component-name"];
            var itemName = restApiMetadata.ClaimsSet["search-item-name"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, itemName);
            this.TestContext.WriteLine("uri: {0}", uri);

            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Add("api-key", restApiMetadata.ApiKey);

            var response = await httpClient.SendAsync(request);
            this.TestContext.WriteLine($"HTTP Status Code: {response.StatusCode}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent, "The expected status code is not here.");
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("uri", "http://ow.ly/87kA3076kM1")]
        public async Task ShouldGet301Or302()
        {
            var uri = new Uri(this.TestContext.Properties["uri"].ToString(), UriKind.Absolute);

            var response = await httpClient.GetAsync(uri);
            this.TestContext.WriteLine("HttpStatusCode: {0}", response.StatusCode);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect, "The expected status code is not here.");
            this.TestContext.WriteLine($"Loaction: {response.Headers.Location}");
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("jsonPath", @"json\ShouldExpandUris.json")]
        [TestProperty("twitterHost", "t.co")]
        public void ShouldExpandUris()
        {
            var projectDirectory = this.TestContext.ShouldGetAssemblyDirectoryParent(this.GetType(), expectedLevels: 2);

            #region test properties:

            var jsonPath = this.TestContext.Properties["jsonPath"].ToString();
            jsonPath = Path.Combine(projectDirectory, jsonPath);
            this.TestContext.ShouldFindFile(jsonPath);

            var twitterHost = this.TestContext.Properties["twitterHost"].ToString();

            #endregion

            var json = File.ReadAllText(jsonPath);
            var blogEntry = JsonConvert.DeserializeObject<BlogEntry>(json);
            Assert.IsNotNull(blogEntry, "The expected Blog Entry is not here.");

            var xml = HtmlUtility.ConvertToXml(blogEntry.Content);
            var xd = XDocument.Parse(string.Format("<content>{0}</content>", xml));

            var twitterAnchors = xd.Descendants("a")
                .Where(i => i.Value.Contains(string.Format("://{0}/", twitterHost)));
            Assert.IsTrue(twitterAnchors.Any(), string.Format("The expected {0} anchors are not here.", twitterHost));

            twitterAnchors.ForEachInEnumerable(async i =>
            {
                var uri = new Uri(i.Attribute("href").Value, UriKind.Absolute);
                this.TestContext.WriteLine("wrapped uri: {0}", uri);

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowAutoRedirect = false;

                var response = await httpClient.GetAsync(uri);
                this.TestContext.WriteLine("HttpStatusCode: {0}", response.StatusCode);
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect, "The expected status code is not here.");

                var expandedUri = response.Headers.Location;
                this.TestContext.WriteLine("expanded URI: {0}", expandedUri);

                i.ExpandTwitterAnchor(expandedUri.OriginalString, twitterHost);
            });

            var xhtml = xd.ToString().Replace(@"</a><a", "</a> <a");

            this.TestContext.WriteLine("XHTML:\n{0}", xhtml);
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("htmlPath", @"content\ShouldGenerateBlogEntryAndUpdateIndex.html")]
        [TestProperty("twitterHost", "t.co")]
        [TestProperty("hootsuiteHost", "ow.ly")]
        public void ShouldExpandUrisFromNewEntry()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var htmlPath = this.TestContext.Properties["htmlPath"].ToString();
            htmlPath = Path.Combine(projectDirectoryInfo.FullName, htmlPath);
            this.TestContext.ShouldFindFile(htmlPath);

            var twitterHost = this.TestContext.Properties["twitterHost"].ToString();
            var hootsuiteHost = this.TestContext.Properties["hootsuiteHost"].ToString();

            #endregion

            var html = File.ReadAllText(htmlPath);
            var xml = HtmlUtility.ConvertToXml(html);
            var xd = XDocument.Parse(xml);

            #region functional members:

            async Task<Uri> expandUri(Uri expandableUri)
            {
                this.TestContext.WriteLine("expanding wrapped URI: {0}...", expandableUri);

                var response = await httpClient.GetAsync(expandableUri);
                this.TestContext.WriteLine("HttpStatusCode: {0}", response.StatusCode);
                if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Redirect)
                {
                    var expandedUri = response.Headers.Location;
                    this.TestContext.WriteLine("expanded URI: {0}", expandedUri);
                    return expandedUri;
                }
                else
                {
                    this.TestContext.WriteLine("The expected status code is not here.");
                    return expandableUri;
                }
            }

            bool isHost(string context, string host) => context.Contains(string.Format("://{0}/", host));

            #endregion

            this.TestContext.WriteLine("looking for Twitter anchors of the form <a*>*://{0}//*</a>...", twitterHost);
            var twitterAnchors = xd.Descendants("a").Where(i => isHost(i.Value, twitterHost));
            Assert.IsTrue(twitterAnchors.Any(), string.Format("The expected {0} anchors are not here.", twitterHost));

            twitterAnchors.ForEachInEnumerable(async i =>
            {
                var uri = new Uri(i.Attribute("href").Value, UriKind.Absolute);
                var expandedUri = await expandUri(uri);
                if (isHost(expandedUri?.OriginalString, hootsuiteHost))
                {
                    expandedUri = await expandUri(expandedUri);
                }

                i.ExpandTwitterAnchor(expandedUri.OriginalString, twitterHost);
            });

            #region functional members:

            string addSpaceBetweenAnchors(string s)
            {
                return s.Replace("</a><", "</a> <");
            }

            string removeNewLineAndSpaceAfterParagraphElement(string s)
            {
                var re = new Regex(@"(\<p\>)\r\n\s+");
                re.Matches(s).OfType<Match>().ForEachInEnumerable(i =>
                {
                    s = s.Replace(i.Value, i.Groups[1].Value);
                });

                return s;
            }

            string removeNewLineAndSpaceBeforeAnchorElement(string s)
            {
                var re = new Regex(@"\r\n\s+(\<a [^\>]+\>)");
                re.Matches(s).OfType<Match>().ForEachInEnumerable(i =>
                {
                    s = s.Replace(i.Value, i.Groups[1].Value);
                });

                return s;
            }

            string removeNewLineAndSpaceBeforeImageElement(string s)
            {
                var re = new Regex(@"\r\n\s+(\<img [^\>]+\>)\r\n\s+");
                re.Matches(s).OfType<Match>().ForEachInEnumerable(i =>
                {
                    s = s.Replace(i.Value, i.Groups[1].Value);
                });

                return s;
            }

            #endregion

            xml = addSpaceBetweenAnchors(xd.ToString());
            xml = removeNewLineAndSpaceAfterParagraphElement(xml);
            xml = removeNewLineAndSpaceBeforeImageElement(xml);
            xml = removeNewLineAndSpaceBeforeAnchorElement(xml);

            File.WriteAllText(htmlPath, xml);
        }

        static readonly HttpClient httpClient;

        static RestApiMetadata restApiMetadata;
    }
}
