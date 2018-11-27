using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public partial class HttpWebRequestTest
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

            cloudStorageMeta = meta.CloudStorageSet["SonghayCloudStorage"];
            Assert.IsNotNull(cloudStorageMeta, "The expected connection string is not here.");

            azureSearchPostTemplate = configuration
                .GetSection(nameof(AzureSearchPostTemplate))
                .Get<AzureSearchPostTemplate>();
            Assert.IsNotNull(azureSearchPostTemplate, "The expected Azure Search template is not here.");
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("uri", "https://t.co/Ok1vhFXZy7")]
        public async Task ShouldGet301Or302()
        {
            var uri = new Uri(this.TestContext.Properties["uri"].ToString(), UriKind.Absolute);

            var response = await httpClient.GetAsync(uri);
            this.TestContext.WriteLine("HttpStatusCode: {0}", response.StatusCode);
            Assert.IsTrue(response.IsMovedOrRedirected(), "The expected status code is not here.");
            this.TestContext.WriteLine($"Location: {response.Headers.Location}");
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("jsonPath", @"json\ShouldExpandUris.json")]
        [TestProperty("twitterHost", "t.co")]
        public async Task ShouldExpandUris()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var jsonPath = this.TestContext.Properties["jsonPath"].ToString();
            jsonPath = projectDirectoryInfo.FullName.ToCombinedPath(jsonPath);
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

            var tasks = twitterAnchors.Select(async i =>
            {
                var uri = new Uri(i.Attribute("href").Value, UriKind.Absolute);
                this.TestContext.WriteLine("wrapped uri: {0}", uri);

                var response = await httpClient.GetAsync(uri);
                this.TestContext.WriteLine("HttpStatusCode: {0}", response.StatusCode);
                Assert.IsTrue(response.IsMovedOrRedirected(), "The expected status code is not here.");

                var expandedUri = response.Headers.Location;
                this.TestContext.WriteLine("expanded URI: {0}", expandedUri);

                i.ExpandTwitterAnchor(expandedUri.OriginalString, twitterHost);
            });
            await Task.WhenAll(tasks);

            var xhtml = xd.ToString().Replace(@"</a><a", "</a> <a");

            this.TestContext.WriteLine("XHTML:\n{0}", xhtml);
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("htmlPath", @"html\ShouldGenerateBlogEntry.html")]
        [TestProperty("twitterHost", "t.co")]
        [TestProperty("hootsuiteHost", "ow.ly")]
        public async Task ShouldExpandUrisFromNewEntry()
        {
            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var htmlPath = this.TestContext.Properties["htmlPath"].ToString();
            htmlPath = projectDirectoryInfo.FullName.ToCombinedPath(htmlPath);
            this.TestContext.ShouldFindFile(htmlPath);

            var twitterHost = this.TestContext.Properties["twitterHost"].ToString();
            var hootsuiteHost = this.TestContext.Properties["hootsuiteHost"].ToString();

            #endregion

            #region functional members:

            string expandAmpersandGlyph(string s)
            {
                var re = new Regex(@"&\s+");
                re.Matches(s).OfType<Match>().ForEachInEnumerable(i =>
                {
                    s = s.Replace(i.Value, "&amp; ");
                });

                return s;
            }

            string expandArrows(string s)
            {
                s = s.Replace("=>", "=&gt;");
                s = s.Replace("<=", "&lt;=");
                return s;
            }

            string expandComparisonOperatorGlyphs(string s)
            {
                var re = new Regex(@"\s(<|>)\s");
                re.Matches(s).OfType<Match>().ForEachInEnumerable(i =>
                {
                    if (i.Groups.Count() != 2) return;
                    var @value = i.Groups[1].Value;
                    if (@value == "<") s = s.Replace(@value, "&lt;");
                    if (@value == ">") s = s.Replace(@value, "&gt;");
                });

                return s;
            }

            string expandGenericNotation(string s)
            {
                s = s.Replace("<T>", "&lt;T&gt;");
                return s;
            }

            async Task<Uri> expandUri(Uri expandableUri)
            {
                var messageBuilder = new StringBuilder();

                messageBuilder.AppendLine($"expanding wrapped URI: {expandableUri}...");

                var response = await httpClient.GetAsync(expandableUri);
                messageBuilder.AppendLine($"HttpStatusCode: {response.StatusCode}");
                if (response.IsMovedOrRedirected())
                {
                    var expandedUri = response.Headers.Location;
                    messageBuilder.AppendLine($"expanded URI: {expandedUri}");

                    this.TestContext.WriteLine(messageBuilder.ToString());

                    return expandedUri;
                }
                else
                {
                    messageBuilder.AppendLine($"WARNING: the expected status code is not here. Headers location: {response.Headers.Location}");

                    this.TestContext.WriteLine(messageBuilder.ToString());

                    return expandableUri;
                }
            }

            bool isHost(string context, string host) => context.Contains($"://{host}/");

            async Task processTwitterAnchorsAsync(XDocument xDocument)
            {
                this.TestContext.WriteLine("looking for Twitter anchors of the form <a*>*://{0}//*</a>...", twitterHost);
                var twitterAnchors = xDocument.Descendants("a").Where(i => isHost(i.Value, twitterHost));

                var tasks = twitterAnchors.Select(async i =>
                {
                    var uri = new Uri(i.Attribute("href").Value, UriKind.Absolute);
                    var expandedUri = await expandUri(uri);
                    if (isHost(expandedUri?.OriginalString, hootsuiteHost))
                    {
                        expandedUri = await expandUri(expandedUri);
                    }

                    i.ExpandTwitterAnchor(expandedUri.OriginalString, twitterHost);
                });
                await Task.WhenAll(tasks);
            }

            string removeAnchorLineBreaks(string s)
            {
                var regexes = new[]
                {
                    new Regex(@"(\r\n?\s*)<a "),
                    new Regex(@"</a>\s*(\r\n?\s*)")
                };

                regexes.ForEachInEnumerable(re =>
                {
                    s = re.Replace(s, match =>
                     {
                         if (match.Groups.Count() != 2) return match.Value;
                         return match.Value.Replace(match.Groups[1].Value, " ");
                     });
                });

                return s;
            }

            string removeImageLineBreaks(string s)
            {
                var re = new Regex(@"(\r\n?\s*)(<img [^>]+/>)\s*(\r\n?\s*)");

                s = re.Replace(s, match =>
                {
                    if (match.Groups.Count() != 4) return match.Value;
                    return $" {match.Groups[2].Value} ";
                });

                return s;
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

            string removeNonBreakingSpace(string s)
            {
                s = s.Replace("&nbsp;", " ");

                return s;
            }

            #endregion

            var html = File.ReadAllText(htmlPath);
            html = expandAmpersandGlyph(html);
            html = expandArrows(html);
            html = expandComparisonOperatorGlyphs(html);
            html = expandGenericNotation(html);
            html = removeNonBreakingSpace(html);
            html = HtmlUtility.ConvertToXml(html);

            var xd = XDocument.Parse(html);
            await processTwitterAnchorsAsync(xd);
            html = xd.ToString();
            html = removeAnchorLineBreaks(html);
            html = removeImageLineBreaks(html);
            html = removeNewLineAndSpaceAfterParagraphElement(html);
            File.WriteAllText(htmlPath, html);
        }

        const string apiKeyHeader = "api-key";

        static readonly HttpClient httpClient;

        static AzureSearchPostTemplate azureSearchPostTemplate;
        static RestApiMetadata restApiMetadata;
        static Dictionary<string, string> cloudStorageMeta;
    }
}
