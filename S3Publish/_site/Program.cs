using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Publish
{
    class Program
    {
        public static string SiteDir = @"\_site";
        static void Main(string[] args)
        {
            //var path = Directory.GetCurrentDirectory();
            var path = @"C:\dev\FunnelWebMigrator\jekyll";

            var sitePath = path + SiteDir;
            if (!Directory.Exists(sitePath))
            {
                Console.WriteLine("Unable to find _site directory under the current root");
                return;
            }

            var browser = new DirectoryBrowser();
            var localFiles = browser.GetAllFiles(sitePath);

            var amazonService = new AmazonService();
            var s3Objects = amazonService.GetListOfAllObjects();
            var compareResults = new ContentComparer().CompareLocalToS3(localFiles, s3Objects);

            if (NeedToSync(compareResults))
            {
                var filesToSync = compareResults.Where(p => p.Status != CompareStatus.Retained).ToList();
                OutputToConsoleFilesToSync(compareResults);
                amazonService.SyncModifiedFiles(filesToSync, sitePath);
            }
            else
            {
                Console.WriteLine("All up to date");
            }
        }

        private static void OutputToConsoleFilesToSync(IEnumerable<CompareResult> compareResults)
        {
            foreach (var compareResult in compareResults)
            {
                Console.WriteLine("{0}: {1}", compareResult.Status, compareResult.Key);
            }
        }

        private static bool NeedToSync(IEnumerable<CompareResult> compareResults)
        {
            return compareResults.Any(p => !p.Status.Equals(CompareStatus.Retained));
        }
    }
}
