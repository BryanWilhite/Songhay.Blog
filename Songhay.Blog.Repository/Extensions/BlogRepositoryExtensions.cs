using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Xml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Songhay.Blog.Repository.Extensions
{
    /// <summary>
    /// Extensions of <see cref="BlogRepository"/>
    /// </summary>
    public static class BlogRepositoryExtensions
    {
        /// <summary>
        /// Converts <see cref="IEnumerable{BlogEntry}"/> to Index in JSON format.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicsPath">The topics path.</param>
        /// <param name="useJavaScriptCase">if set to <c>true</c> [use java script case].</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">
        /// The expected index data is not here.
        /// </exception>
        /// <exception cref="FileNotFoundException">The expected Index topics are not here.</exception>
        /// <exception cref="FormatException">The expected Blog entries are not here.</exception>
        public static async Task<string> ToRepositoryIndexAsync(this BlogRepository repository, string topicsPath, bool useJavaScriptCase)
        {
            if (repository == null) throw new NullReferenceException();
            if (!File.Exists(topicsPath)) throw new FileNotFoundException("The expected Index topics are not here.");

            var entries = await repository.LoadAllAsync<BlogEntry>();
            if (!entries.Any()) throw new FormatException("The expected Blog entries are not here.");

            var xd = XDocument.Load(topicsPath);
            var topics = OpmlUtility.GetDocument(xd.Root, OpmlUtility.rx);

            var json = entries.GenerateIndex(topics, useJavaScriptCase);
            if (string.IsNullOrEmpty(json)) throw new NullReferenceException("The expected index data is not here.");

            return json;
        }
    }
}
