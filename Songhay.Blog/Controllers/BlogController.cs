using Microsoft.AspNetCore.Mvc;
using Songhay.Blog.Models;
using Songhay.Blog.Models.Extensions;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using System;
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
            .GetConfiguredTraceSource()
            .WithSourceLevels();

        static readonly TraceSource traceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogApiController"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public BlogController(IRepositoryAsync repository)
        {
            this._repository = repository;
            traceSource?.TraceVerbose($"initializing {this.GetType().Name}...");
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

            traceSource?.TraceVerbose("Getting repository entry...");
            try
            {
                var blogEntry = await this._repository.LoadSingleAsync<BlogEntry>(id.ToLowerInvariant());
                traceSource?.TraceVerbose("Returning repository entry...");
                return this.Ok(blogEntry);
            }
            catch (FileNotFoundException ex)
            {
                traceSource?.TraceError("The expected repository entry ID {0} is not here. Throwing a 404...", id);
                traceSource?.TraceError(ex);
                return this.NotFound();
            }
        }

        /// <summary>
        /// Gets the blog entry as permalink.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/[controller]/entry/permalink/{id}")]
        public async Task<IActionResult> GetBlogEntryAsPermaLinkAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return this.BadRequest();
            try
            {
                var blogEntry = await this._repository.LoadSingleAsync<BlogEntry>(id.ToLowerInvariant());
                traceSource?.TraceVerbose("Returning repository entry...");
                return this.View("PermaLink", blogEntry.WithGitHubDirectivesTranscluded());
            }
            catch (FileNotFoundException ex)
            {
                traceSource?.TraceError("The expected repository entry ID {0} is not here. Throwing a 404...", id);
                traceSource?.TraceError(ex);
                return this.NotFound();
            }
        }

        /// <summary>
        /// Gets the index of <see cref="BlogEntry"/>.
        /// </summary>
        /// <returns></returns>
        [Route("index")]
        public async Task<ActionResult> GetIndexAsync()
        {
            var data = await (this._repository as IBlogEntryIndex).GetIndexAsync();
            return View("Index", data);
        }

        /// <summary>
        /// Gets the inline-framing of GitHub Gist.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Route("/Inline/GitHubGist/{id}")]
        public IActionResult GetInlineFramingOfGitHubGist(string id)
        {
            traceSource?.TraceVerbose($"{nameof(this.GetInlineFramingOfGitHubGist)} ID: {0}", id);

            var data = new DisplayItemModel
            {
                ResourceIndicator = new Uri($"https://gist.github.com/BryanWilhite/{id}.js", UriKind.Absolute)
            };
            return View("GitHubGist", data);
        }

        /// <summary>
        /// Redirects to angular client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/Index/Entry/Show/{id}")]
        public IActionResult RedirectToAngularClient(string id)
        {
            traceSource?.TraceVerbose($"{nameof(this.RedirectToAngularClient)} ID: {id}");

            return this.RedirectPermanent($"~/blog/entry/{id}");
        }

        readonly IRepositoryAsync _repository;
    }
}
