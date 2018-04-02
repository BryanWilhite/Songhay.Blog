using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Songhay.Blog.Models;
using Songhay.Cloud.BlobStorage.Models;
using Songhay.Cloud.BlobStorage.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Songhay.Blog.Repository
{
    /// <summary>
    /// Defines a JSON blob Repository for Azure Storage
    /// with JSON index support.
    /// </summary>
    public class BlogRepository : AzureBlobRepository, IBlogEntryIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogRepository"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="container">The container.</param>
        public BlogRepository(AzureBlobKeys keys, CloudBlobContainer container)
            : base(keys, container)
        {
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <exception cref="System.NullReferenceException">The expected repository container is not here.</exception>
        public async Task<IEnumerable<BlogEntry>> GetIndexAsync()
        {
            if (this.GetCloudBlobContainer() == null) throw new NullReferenceException("The expected repository container is not here.");

            var reference = this.GetIndexReference();
            await this.CheckReferenceAsync(reference, indexKey);

            var json = await reference.DownloadTextAsync();
            var entity = JsonConvert.DeserializeObject<IEnumerable<BlogEntry>>(json);
            return entity;
        }

        /// <summary>
        /// Sets the index.
        /// </summary>
        /// <param name="json">The json.</param>
        public async Task SetIndex(string json)
        {
            var reference = this.GetIndexReference();
            reference.Properties.ContentType = "application/json";
            await reference.UploadTextAsync(json);
        }
    }
}
