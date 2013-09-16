using System.Collections.Generic;
using System.Linq;
using Amazon.S3.Model;

namespace S3Publish
{
    public class ContentComparer
    {
        public List<CompareResult> CompareLocalToS3(List<LocalFile> localFiles, List<S3Object> s3Objects)
        {
            var results = new List<CompareResult>();
            foreach (var localFile in localFiles)
            {
                var s3Object = s3Objects.SingleOrDefault(p => p.Key == localFile.Key);
                if (s3Object == null)
                {
                    results.Add(new CompareResult(localFile.Key, CompareStatus.Added));
                    continue;
                }

                results.Add(s3Object.ETag == localFile.ETag
                    ? new CompareResult(localFile.Key, CompareStatus.Retained)
                    : new CompareResult(localFile.Key, CompareStatus.Modified));
            }

            var deleted = from s3 in s3Objects
                          join p in localFiles
                    on s3.Key equals p.Key into pp
                from p1 in pp.DefaultIfEmpty()
                where p1 == null
                select new CompareResult(s3.Key, CompareStatus.Deleted);

            results.AddRange(deleted);
            return results;
        }
    }
}