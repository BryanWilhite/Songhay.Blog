using Microsoft.AspNetCore.Mvc;
using Songhay.Blog.Models;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using System.Diagnostics;
using System.IO;

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
        public BlogController(IRepository repository)
        {
            this._repository = repository;
            this._repositoryIndex = repository as IBlogEntryIndex;
            traceSource.TraceVerbose("initializing {0}", this.GetType().Name);
        }

        /// <summary>
        /// Gets the blog entry.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.Routing.Constraints.MinRouteConstraint"></exception>
        [Route("entry/{id}")]
        public IActionResult GetBlogEntry(string id)
        {

            if (string.IsNullOrEmpty(id)) return this.BadRequest();

            traceSource.TraceVerbose("Getting repository entry...");
            try
            {
                var blogEntry = this._repository.LoadSingle<BlogEntry>(id.ToLowerInvariant());
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

        readonly IRepository _repository;
        readonly IBlogEntryIndex _repositoryIndex;
    }
}
