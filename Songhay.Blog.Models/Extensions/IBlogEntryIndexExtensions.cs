using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Songhay.Blog.Models.Extensions
{
    /// <summary>
    /// Extensions of <see cref="IBlogEntryIndex"/>
    /// </summary>
    public static class IBlogEntryIndexExtensions
    {
        static IBlogEntryIndexExtensions() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        /// <summary>
        /// Generates the index.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="entries">The entries.</param>
        /// <param name="topics">The topics.</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">
        /// The expected Blog entries are not here.
        /// or
        /// The expected Blog entries are not here.
        /// or
        /// The expected Blog topics are not here.
        /// </exception>
        public static string GenerateIndex(this IBlogEntryIndex repository, IEnumerable<BlogEntry> entries, OpmlDocument topics)
        {
            if (entries == null) throw new NullReferenceException("The expected Blog entries are not here.");
            if (!entries.Any()) throw new NullReferenceException("The expected Blog entries are not here.");

            traceSource.TraceInformation("Generating Blog Index...");

            var entriesLite = entries.Select(i =>
            {
                var entry = i.ToMemberwiseClone()
                    .WithItemCategory(topics)
                    .WithContentAsExtract();

                traceSource.TraceVerbose("entry: {0}", entry);

                return entry;
            });


            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(entriesLite, serializerSettings);
            return json;
        }

        /// <summary>
        /// Adds the default slug, fluently.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public static BlogEntry WithDefaultSlug(this BlogEntry entry)
        {
            if (entry == null) return null;
            if (string.IsNullOrEmpty(entry.Title)) return entry;

            entry.Slug = entry.Title.ToBlogSlug();

            return entry;
        }

        static readonly TraceSource traceSource;
    }
}
