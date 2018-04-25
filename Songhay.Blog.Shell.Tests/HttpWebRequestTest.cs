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
using System.Xml.Linq;

namespace Songhay.Blog.Shell.Tests
{
    [TestClass]
    public class HttpWebRequestTest
    {
        static HttpWebRequestTest()
        {
            httpClient = new HttpClient();
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

        [TestMethod]
        public void TestMethod1()
        {
        }

        static readonly HttpClient httpClient;

        static RestApiMetadata restApiMetadata;
    }
}
