using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Publish
{
    public class AmazonService
    {
        public static string BucketName = ConfigurationManager.AppSettings["BucketName"];
        public static string BrowserCacheExpiration = ConfigurationManager.AppSettings["Cache-Control.max-age"];
        public bool AddCacheControlHeaderToStaticFiles { get; set; }

        public AmazonService()
        {
            AddCacheControlHeaderToStaticFiles = true;
        }

        public List<S3Object> GetListOfAllObjects()
        {
            var client = new AmazonS3Client();
            var request = new ListObjectsRequest { BucketName = BucketName };
            return client.ListObjects(request).S3Objects;
        }

        public void SyncModifiedFiles(List<CompareResult> results, string rootPath)
        {
            var deleted = results.Where(p => p.Status == CompareStatus.Deleted).ToArray();
            if (deleted.Any())
            {
                DeleteS3Objects(deleted);
            }

            foreach (var result in results)
            {
                var path = rootPath.EndsWith("\\") ? rootPath : rootPath + "\\" + result.Key;
                if (result.Status == CompareStatus.Added
                    || result.Status == CompareStatus.Modified)
                {
                    PutObjectToS3(result.Key, path);
                }
            }
        }

        private void DeleteS3Objects(CompareResult[] deleted)
        {
            var deleteRequest = new DeleteObjectsRequest();
            deleteRequest.BucketName = BucketName;
            foreach (var deletedFile in deleted)
            {
                deleteRequest.AddKey(deletedFile.Key);
            }
            using (var client = new AmazonS3Client())
            {
                var response = client.DeleteObjects(deleteRequest);
                Console.WriteLine("Successfully deleted all the {0} items", response.DeletedObjects.Count);
            }
        }

        private void PutObjectToS3(string key, string path)
        {
            PutObjectRequest request = new PutObjectRequest();

            request.BucketName = BucketName;
            request.Key = key;
            request.FilePath = path;
            if (AddCacheControlHeaderToStaticFiles && !key.EndsWith(".html"))
            {
                request.AddHeader("Cache-Control", "max-age=" + BrowserCacheExpiration);
            }

            Console.Out.WriteLine("Writing S3 object with key " + key + " in bucket " + BucketName);
            using (var client = new AmazonS3Client())
            {
                client.PutObject(request);
            }
            
        }
    }
}