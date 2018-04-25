using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Songhay.Blog.Repository.Tests.Extensions;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System.Diagnostics;
using System.IO;

namespace Songhay.Blog.Repository.Tests
{
    [TestClass]
    public class BlogRepositoryTest
    {
        static BlogRepositoryTest() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithAllSourceLevels()
            .EnsureTraceSource();

        static readonly TraceSource traceSource;

        [TestInitialize]
        public void InitializeTest()
        {
            var basePath = FrameworkFileUtility.FindParentDirectory(Directory.GetCurrentDirectory(), this.GetType().Namespace, 5);
            cloudStorageAccount = this.TestContext.ShouldGetCloudStorageAccount(basePath);
        }

        public TestContext TestContext { get; set; }

        static CloudStorageAccount cloudStorageAccount;
    }
}
