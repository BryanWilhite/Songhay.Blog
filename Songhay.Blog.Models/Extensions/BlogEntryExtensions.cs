using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Songhay.Blog.Models.Extensions
{
    /// <summary>
    /// Extensions of <see cref="BlogEntry"/>
    /// </summary>
    public static class BlogEntryExtensions
    {
        static BlogEntryExtensions() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        /// <summary>
        /// Reduces <see cref="BlogEntry.Content"/> to an extract.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static BlogEntry WithContentAsExtract(this BlogEntry data)
        {
            if (data == null) return null;

            #region functional members:

            Func<string, string> getExtract = content =>
            {
                if (string.IsNullOrEmpty(content)) return null;

                traceSource.TraceVerbose("Getting Blog Entry Content...");
                traceSource.TraceVerbose("Getting Blog Entry Slug: {0}", data.Slug);

                var limit = default(int);

                try
                {
                    limit = 255;
                    if (XmlUtility.IsXml(content))
                    {
                        content = HtmlUtility.ConvertToXml(content);
                        content = content.Replace("&nbsp;", string.Empty); // TODO: this should be in HtmlUtility.ConvertToXml().
                        var rootElement = XElement.Parse(string.Format("<root>{0}</root>", content));
                        content = XObjectUtility.JoinFlattenedXTextNodes(rootElement);
                    }
                }
                catch (Exception ex)
                {
                    traceSource.TraceError(ex);
                }

                return (content.Length > limit) ? string.Format("{0}...", content.Trim().Substring(0, limit - 1)) : content;
            };

            #endregion

            data.Content = getExtract(data.Content);

            return data;
        }

        /// <summary>
        /// Transforms and includes any output from <c>rx-iframe</c> GitHub directives.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static BlogEntry WithGitHubDirectivesTranscluded(this BlogEntry data)
        {
            if (data == null) return null;
            data.Content = Regex.Replace(data.Content, @"<rx-iframe src=""./Inline/GitHubGist/([^""]+)""></rx-iframe>", match =>
            {
                if (match.Groups.Count != 2) return match.Value;
                var id = match.Groups[1].Value;
                return string.Format(@"<script src=""https://gist.github.com/BryanWilhite/{0}.js""></script>", id);
            });

            return data;
        }

        /// <summary>
        /// Sets <see cref="BlogEntry.ItemCategory"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="topics">The topics.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">The expected Blog topics are not here.</exception>
        public static BlogEntry WithItemCategory(this BlogEntry data, OpmlDocument topics)
        {
            if (data == null) return null;
            if (topics == null) throw new NullReferenceException("The expected Blog topics are not here.");

            #region functional members:

            Func<OpmlOutline[], string[]> getCompoundKeywords = outlines =>
            {
                return outlines
                    .Where(j => j.Category == "compoundkeyword")
                    .Select(j => j.Text.Replace("_", " "))
                    .ToArray();
            };

            Func<OpmlOutline[], string, IEnumerable<string>> getCuratedTopics = (outlines, title) =>
            {
                return outlines.Where(i =>
                {
                    var compoundKeywords = getCompoundKeywords(i.Outlines);
                    compoundKeywords.ForEachInEnumerable(j => title = title.Replace(j, j.Replace(" ", "_")));

                    var titleWords = title
                        .Split(' ')
                        .Where(j => !string.IsNullOrWhiteSpace(j));

                    var keywords = i.Outlines.Select(j => j.Text).ToArray();
                    return titleWords.Intersect(keywords).Any();
                })
                    .Select(i => string.Format(@"""{0}"":""{1}""", i.Id, i.Text));
            };

            Func<string, string> stripDownTitle = title =>
            {
                if (string.IsNullOrEmpty(title)) return null;

                var s = title.ToLower()
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Replace("“", string.Empty)
                    .Replace("”", string.Empty)
                    .Replace("‘", string.Empty)
                    .Replace("’", " ")
                    .Replace("–", " ")
                    .Replace("—", " ")
                    .Replace("…", " ")
                    .Replace(",", string.Empty)
                    .Replace(".", " ")
                    .Replace(";", string.Empty)
                    .Replace(":", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("!", string.Empty)
                    .Replace("#", " ")
                    .Replace("/", " ");
                return s;
            };

            Func<BlogEntry, string> getCategory = entry =>
            {
                var year = entry.InceptDate.Year;
                var month = entry.InceptDate.Month.ToString("00");
                var day = entry.InceptDate.Day.ToString("00");
                var categories = string.Format(@"""year"":{0},""month"":""{1}"",""day"":""{2}"",""dateGroup"":""{0}\/{1}""", year, month, day);

                var title = stripDownTitle(entry.Title);
                var curatedTopics = getCuratedTopics(topics.OpmlBody.Outlines, title);
                if (curatedTopics.Any()) categories = string.Concat(categories, ",", string.Join(",", curatedTopics));

                return categories;
            };

            #endregion

            data.ItemCategory = getCategory(data);

            return data;
        }

        static readonly TraceSource traceSource;
    }
}
