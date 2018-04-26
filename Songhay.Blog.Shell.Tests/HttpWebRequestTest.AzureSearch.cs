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
    public partial class HttpWebRequestTest
    {
        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        public async Task ShouldDeleteAzureSearchServiceComponent()
        {
            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];
            var itemName = restApiMetadata.ClaimsSet["search-item-name"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, itemName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey);

            var response = await httpClient.SendAsync(request);
            this.TestContext.WriteLine($"HTTP Status Code: {response.StatusCode}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent, "The expected status code is not here.");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("jsonPath", @"json\ShouldGenerateAzureSearchServiceDataSource.json")]
        public async Task ShouldGenerateAzureSearchServiceDataSource()
        {
            var projectDirectory = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());

            #region test properties:

            var jsonPath = this.TestContext.Properties["jsonPath"].ToString();
            jsonPath = Path.Combine(projectDirectory.FullName, jsonPath);
            this.TestContext.ShouldFindFile(jsonPath);

            #endregion

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["endpoint"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-datasources"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var json = File.ReadAllText(jsonPath);
            var jO = JObject.Parse(json);
            jO["credentials"]["connectionString"] = cloudStorageMeta["classic"];
            jO["container"]["name"] = cloudStorageMeta["classic-day-path-container"];
            json = jO.ToString();
            this.TestContext.WriteLine($"JSON payload: {json}");

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("jsonPath", @"json\ShouldGenerateAzureSearchServiceIndex.json")]
        public async Task ShouldGenerateAzureSearchServiceIndex()
        {
            var projectDirectory = this.TestContext.ShouldGetAssemblyDirectoryParent(this.GetType(), expectedLevels: 2);

            #region test properties:

            var jsonPath = this.TestContext.Properties["jsonPath"].ToString();
            jsonPath = Path.Combine(projectDirectory, jsonPath);
            this.TestContext.ShouldFindFile(jsonPath);

            #endregion

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["endpoint"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexes"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var json = File.ReadAllText(jsonPath);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("jsonPath", @"json\ShouldGenerateAzureSearchServiceIndexer.json")]
        public async Task ShouldGenerateAzureSearchServiceIndexer()
        {
            var projectDirectory = this.TestContext.ShouldGetAssemblyDirectoryParent(this.GetType(), expectedLevels: 2);

            #region test properties:

            var jsonPath = this.TestContext.Properties["jsonPath"].ToString();
            jsonPath = Path.Combine(projectDirectory, jsonPath);
            this.TestContext.ShouldFindFile(jsonPath);

            #endregion

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["endpoint"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var json = File.ReadAllText(jsonPath);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }

        [Ignore("This test is meant to run manually on the Desktop.")]
        [TestCategory("Integration")]
        [TestMethod]
        public async Task ShouldGetAzureSearchServiceComponent()
        {
            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];
            var itemName = restApiMetadata.ClaimsSet["search-item-name"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, itemName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var response = await httpClient.DownloadToStringAsync(uri, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {response}");
        }

        const string apiKeyHeader = "api-key";
    }
}
