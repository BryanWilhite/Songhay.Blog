using Microsoft.AspNetCore.Mvc;
using Songhay.Blog.Models;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Songhay.Blog.Controllers
{
    /// <summary>
    /// Controls Songhay Blog
    /// </summary>
    [Route("api/[controller]")]
    public class BlogController : Controller
    {
        static BlogController() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        static readonly TraceSource traceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogApiController"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public BlogController(IRepositoryAsync repository)
        {
            this._repository = repository;
            traceSource.TraceVerbose($"initializing {this.GetType().Name}...");
        }

        /// <summary>
        /// Gets the blog entry.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("entry/{id}")]
        public async Task<IActionResult> GetBlogEntryAsync(string id)
        {

            if (string.IsNullOrEmpty(id)) return this.BadRequest();

            traceSource.TraceVerbose("Getting repository entry...");
            try
            {
                var blogEntry = await this._repository.LoadSingleAsync<BlogEntry>(id.ToLowerInvariant());
                traceSource.TraceVerbose("Returning repository entry...");
                return this.Ok(blogEntry);
            }
            catch (FileNotFoundException ex)
            {
                traceSource.TraceError("The expected repository entry ID {0} is not here. Throwing a 404...", id);
                traceSource.TraceError(ex);
                return this.NotFound();
            }
        }

        readonly IRepositoryAsync _repository;
    }
}
