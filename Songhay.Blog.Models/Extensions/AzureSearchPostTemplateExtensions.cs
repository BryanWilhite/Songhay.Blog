using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Songhay.Blog.Models.Extensions
{
    /// <summary>
    /// Extensions of <see cref="AzureSearchPostTemplate"/>
    /// </summary>
    public static class AzureSearchPostTemplateExtensions
    {
        /// <summary>
        /// Returns <see cref="AzureSearchPostTemplate"/>
        /// with conventional, default values.
        /// </summary>
        /// <param name="template">The meta.</param>
        /// <returns></returns>
        public static AzureSearchPostTemplate WithDefaultValues(this AzureSearchPostTemplate template)
        {
            if (template == null) template = new AzureSearchPostTemplate();

            template.Count = true;
            template.MinimumCoverage = 100;
            template.QueryType = "simple";
            template.Search = "{searchText}";
            template.SearchFields = "Content, Title";
            template.SearchMode = "any";
            template.Select = "*";
            template.Skip = "{skipValue}";
            template.Top = 10;

            return template;
        }

        /// <summary>
        /// Converts <see cref="AzureSearchPostTemplate"/>
        /// to a JSON <see cref="string"/>.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">The expected Azure Search template is not here.</exception>
        public static string ToJson(this AzureSearchPostTemplate template)
        {
            if (template == null) throw new NullReferenceException("The expected Azure Search template is not here.");

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(template, serializerSettings);

            return json;
        }
    }
}
