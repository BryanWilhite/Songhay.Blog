using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Songhay.Blog.Models.Extensions;
using Songhay.Extensions;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Songhay.Blog.Shell.Tests
{
    public partial class HttpWebRequestTest
    {
        [TestCategory("Integration")]
        [TestMethod]
        public async Task ShouldDeleteAzureSearchServiceComponent()
        {
            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component-item"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];
            var itemName = restApiMetadata.ClaimsSet["search-item-indexer-name"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, itemName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var response = await httpClient.DeleteAsync(uri, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"HTTP Status Code: {response.StatusCode}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent, "The expected status code is not here.");
        }

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

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component"]);
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

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexes"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var json = File.ReadAllText(jsonPath);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }

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

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var json = File.ReadAllText(jsonPath);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task ShouldGetAzureSearchServiceComponent()
        {
            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component-item"]);
            var apiVersion = restApiMetadata.ClaimsSet["search-api-version"];
            var componentName = restApiMetadata.ClaimsSet["component-name-indexers"];
            var itemName = restApiMetadata.ClaimsSet["search-item-indexer-name"];

            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, componentName, itemName, apiVersion);
            this.TestContext.WriteLine("uri: {0}", uri);

            var response = await httpClient.DownloadToStringAsync(uri, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {response}");
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("searchText", "ASP.NET")]
        [TestProperty("skipValue", "10")]
        public async Task ShouldSearchAzureSearchIndex()
        {
            #region test properties:

            var searchText = this.TestContext.Properties["searchText"].ToString();
            var skipValue = Convert.ToInt32(this.TestContext.Properties["skipValue"]);

            #endregion

            azureSearchPostTemplate.Search = searchText;
            azureSearchPostTemplate.Skip = skipValue;

            var json = azureSearchPostTemplate.ToJson();
            this.TestContext.WriteLine("query: {0}", json);

            var apiTemplate = new UriTemplate(restApiMetadata.UriTemplates["search-component-item"]);
            var itemName = restApiMetadata.ClaimsSet["search-item-index-name"];
            var uri = apiTemplate.BindByPosition(restApiMetadata.ApiBase, itemName);
            this.TestContext.WriteLine("uri: {0}", uri);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, restApiMetadata.ApiKey));
            this.TestContext.WriteLine($"response: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
