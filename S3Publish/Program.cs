using System;
using System.Configuration;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Publish
{
    class Program
    {
        public static string SiteDir = @"\_site";
        static void Main(string[] args)
        {
            var path = Directory.GetCurrentDirectory();
            var sitePath = path + SiteDir;
            if (!Directory.Exists(sitePath))
            {
                Console.WriteLine("Unable to find _site directory under the current root");
                return;
            }

            var browser = new DirectoryBrowser();
            var localFiles = browser.GetAllFiles(sitePath);

            var client = new AmazonS3Client();
            //var response = client.ListBuckets();
            
            var request = new ListObjectsRequest { BucketName = ConfigurationManager.AppSettings["BucketName"] };
            var s3Objects = client.ListObjects(request).S3Objects;

            var compareResult = new ContentComparer().CompareLocalToS3(localFiles, s3Objects);

        }
    }
}
