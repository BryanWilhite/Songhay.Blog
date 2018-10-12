using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Songhay.Blog.Models;
using Songhay.Models;
using System;
using System.IO;
using System.Linq;

namespace Songhay.Blog.Repository.Extensions
{
    /// <summary>
    /// Extensions of <see cref="ConfigurationBuilder"/>
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        public static CloudStorageAccount ToCloudStorageAccount(this ConfigurationBuilder builder, string basePath)
        {
            return builder.ToCloudStorageAccount(basePath, AppScalars.cloudStorageAccountClassic);
        }

        public static CloudStorageAccount ToCloudStorageAccount(this ConfigurationBuilder builder, string basePath, string connectionStringName)
        {
            if (builder == null) throw new NullReferenceException("The expected Configuration builder is not here.");
            if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException("The expected configuration base path is not here.");

            builder
                .SetBasePath(basePath)
                .AddJsonFile(AppScalars.conventionalSettingsFile, optional: false, reloadOnChange: true);

            var meta = new ProgramMetadata();
            builder.Build().Bind(nameof(ProgramMetadata), meta);

            if (meta.CloudStorageSet == null) throw new NullReferenceException("The expected cloud storage set is not here.");

            var key = AppScalars.cloudStorageSetName;
            var test = meta.CloudStorageSet.TryGetValue(key, out var set);
            if (!test) throw new NullReferenceException($"The expected cloud storage set, {key}, is not here.");
            if (!set.Any()) throw new NullReferenceException($"The expected cloud storage set items for {key} are not here.");

            test = set.TryGetValue(connectionStringName, out var connectionString);
            if (!test) throw new NullReferenceException($"The expected cloud storage set connection, {connectionStringName}, is not here.");

            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            return cloudStorageAccount;
        }
    }
}
