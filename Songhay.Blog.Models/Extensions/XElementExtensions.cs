using System;
using System.Xml.Linq;


namespace Songhay.Blog.Models.Extensions
{
    /// <summary>
    /// Extensions of <see cref="XElement"/>
    /// </summary>
    public static class XElementExtensions
    {
        /// <summary>
        /// Expands the twitter anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="expandedUriString">The expanded URI <see cref="string"/>.</param>
        /// <param name="twitterHost">The twitter host.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The expected expanded URI data is not here.
        /// or
        /// The expected Twitter host fragment is not here.
        /// </exception>
        public static void ExpandTwitterAnchor(this XElement anchor, string expandedUriString, string twitterHost)
        {
            if (string.IsNullOrEmpty(expandedUriString)) throw new ArgumentNullException("The expected expanded URI data is not here.");
            if (string.IsNullOrEmpty(twitterHost)) throw new ArgumentNullException("The expected Twitter host fragment is not here.");

            var expandedUri = new Uri(expandedUriString, UriKind.Absolute);

            var href = anchor.Attribute("href");
            href.Value = expandedUri.OriginalString;
            if (href.Value.ToLowerInvariant().Contains("http://www.asp.net/"))
            {
                anchor.Value = string.Format("{0}", expandedUri
                    .Host
                    .Replace("www.", string.Empty)
                    .ToUpperInvariant());
            }
            else if (anchor.Value.Contains(twitterHost))
            {
                anchor.Value = string.Format("[{0}]", expandedUri
                    .Host
                    .Replace("www.", string.Empty));
            }
        }
    }
}
