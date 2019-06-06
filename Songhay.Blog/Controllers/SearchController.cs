using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Songhay.Blog.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        static SearchController()
        {
            traceSource = TraceSources
                .Instance
                .GetConfiguredTraceSource()
                .WithSourceLevels();

            httpClient = new HttpClient();
        }

        static readonly TraceSource traceSource;

        public SearchController(RestApiMetadata searchMetadata, AzureSearchPostTemplate searchPostTemplate)
        {
            this._restApiMetadata = searchMetadata;
            this._searchPostTemplate = searchPostTemplate;
        }

        [HttpGet]
        [Route("blog/{searchText}/{skipValue}")]
        public async Task<IActionResult> GetBlogSearchResultAsync(string searchText, int skipValue)
        {
            this._searchPostTemplate.Search = searchText;
            this._searchPostTemplate.Skip = skipValue;

            var json = this._searchPostTemplate.ToJson();
            traceSource?.TraceVerbose("query: {0}", json);

            var apiTemplate = new UriTemplate(this._restApiMetadata.UriTemplates["search-docs"]);
            var indexName = this._restApiMetadata.ClaimsSet["search-item-index-name"];
            var apiVersion = this._restApiMetadata.ClaimsSet["search-api-version"];
            var uri = apiTemplate.BindByPosition(this._restApiMetadata.ApiBase, indexName, apiVersion, searchText);
            var uriBuilder = new UriBuilder(uri.OriginalString.Replace("%3F", "?"));
            uriBuilder.Query = uriBuilder.Query.Replace("api_version", "api-version");
            uri = uriBuilder.Uri;
            traceSource?.TraceVerbose("URI: {0}", uri);

            var response = await httpClient.PostJsonAsync(uri, json, request => request.Headers.Add(apiKeyHeader, this._restApiMetadata.ApiKey));
            var jsonOutput = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                traceSource?.TraceError($"Search POST response: {response.StatusCode}");
                if (string.IsNullOrEmpty(jsonOutput)) traceSource?.TraceError("The expected JSON output is not here.");
                return this.BadRequest();
            }

            var jO = JObject.Parse(jsonOutput);
            return this.Ok(jO);
        }

        const string apiKeyHeader = "api-key";

        static readonly HttpClient httpClient;

        readonly RestApiMetadata _restApiMetadata;
        readonly AzureSearchPostTemplate _searchPostTemplate;
    }
}
