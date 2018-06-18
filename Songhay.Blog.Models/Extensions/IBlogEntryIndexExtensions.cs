using Songhay.Diagnostics;
using Songhay.Extensions;
using System.Diagnostics;

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
