using System.Collections.Generic;
using System.Threading.Tasks;

namespace Songhay.Blog.Models
{
    /// <summary>
    /// Defines index support for <see cref="BlogEntry"/>.
    /// </summary>
    public interface IBlogEntryIndex
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        Task<IEnumerable<BlogEntry>> GetIndexAsync();

        /// <summary>
        /// Sets the index.
        /// </summary>
        /// <param name="json">The json.</param>
        void SetIndex(string json);
    }
}
