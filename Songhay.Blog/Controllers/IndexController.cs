using Microsoft.AspNetCore.Mvc;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System.Diagnostics;

namespace Songhay.Blog.Controllers
{
    /// <summary>
    /// Controls Index-Level Requests
    /// </summary>
    /// <seealso cref="Controller" />
    public class IndexController : Controller
    {
        static IndexController() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        static readonly TraceSource traceSource;

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Redirects to angular client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Entry/Show/{id}")]
        public ActionResult RedirectToAngularClient(string id)
        {
            traceSource.TraceVerbose($"RedirectToAngularClient ID: {id}");

            return this.RedirectPermanent($"~/blog/entry/{id}");
        }
    }
}