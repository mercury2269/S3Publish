using System.Collections.Generic;
using System.Linq;
using Amazon.S3.Model;
using Machine.Specifications;

namespace S3Publish.Specs
{
    [Subject(typeof(ContentComparer))]
    public class when_comparing_local_content_with_same_keys_and_etags_as_s3_content
    {
        static ContentComparer Comparer; 
        static List<LocalFile> LocalFiles = new List<LocalFile>();
        static List<S3Object> S3Objects = new List<S3Object>();
        static List<CompareResult> CompareResults = new List<CompareResult>();
        
        Establish context = () =>
        {
            Comparer = new ContentComparer();
            LocalFiles.Add(new LocalFile { Key = "index.html", ETag = "123"});
            LocalFiles.Add(new LocalFile { Key = "1/about.html", ETag = "567" });
            LocalFiles.Add(new LocalFile { Key = "sitemap.xml", ETag = "567" });
            S3Objects = LocalFiles.Select(p => new S3Object {Key = p.Key, ETag = p.ETag}).ToList();
        };

        Because of = () => CompareResults = Comparer.CompareLocalToS3(LocalFiles, S3Objects);

        private It should_result_in_compare_results_with_retained_status = () =>
        {
            for (int i = 0; i < LocalFiles.Count; i++)
            {
                CompareResults[i].Status.ShouldEqual(CompareStatus.Retained);
                CompareResults[i].Key.ShouldEqual(LocalFiles[i].Key);
            }
        };
    }

    [Subject(typeof (ContentComparer))]
    public class when_content_is_modified
    {
        static ContentComparer Comparer;
        static List<LocalFile> LocalFiles = new List<LocalFile>();
        static List<S3Object> S3Objects = new List<S3Object>();
        static List<CompareResult> CompareResults = new List<CompareResult>();

        Establish context = () =>
        {
            Comparer = new ContentComparer();
            LocalFiles.Add(new LocalFile { Key = "index.html", ETag = "123" });
            S3Objects.Add(new S3Object {Key = "index.html", ETag = "124"});
        };

        Because of = () => CompareResults = Comparer.CompareLocalToS3(LocalFiles, S3Objects);

        private It should_recognized_modified_content =
            () =>
                CompareResults.ShouldContainOnly(
                    new CompareResultsBuilder().Add("index.html", CompareStatus.Modified).Build());
    }

    [Subject(typeof(ContentComparer))]
    public class when_local_items_were_added
    {
        static ContentComparer Comparer;
        static List<LocalFile> LocalFiles = new List<LocalFile>();
        static List<S3Object> S3Objects = new List<S3Object>();
        static List<CompareResult> CompareResults = new List<CompareResult>();

        Establish context = () =>
        {
            Comparer = new ContentComparer();
            LocalFiles.Add(new LocalFile { Key = "index.html", ETag = "123" });
            LocalFiles.Add(new LocalFile { Key = "1/about.html", ETag = "567" });
            S3Objects = LocalFiles.Select(p => new S3Object { Key = p.Key, ETag = p.ETag }).ToList();
            LocalFiles.Add(new LocalFile { Key = "sitemap.xml", ETag = "567" });
            LocalFiles.Add(new LocalFile { Key = "sitemap2.xml", ETag = "567" });
        };

        Because of = () => CompareResults = Comparer.CompareLocalToS3(LocalFiles, S3Objects);

        private It should_recognize_that_new_items_were_added =
            () => CompareResults.ShouldContainOnly(new CompareResultsBuilder()
                .Add("index.html", CompareStatus.Retained)
                .Add("1/about.html", CompareStatus.Retained)
                .Add("sitemap.xml", CompareStatus.Added)
                .Add("sitemap2.xml", CompareStatus.Added).Build());

    }

    [Subject(typeof(ContentComparer))]
    public class when_local_items_are_deleted
    {
        static ContentComparer Comparer;
        static List<LocalFile> LocalFiles = new List<LocalFile>();
        static List<S3Object> S3Objects = new List<S3Object>();
        static List<CompareResult> CompareResults = new List<CompareResult>();

        Establish context = () =>
        {
            Comparer = new ContentComparer();
            LocalFiles.Add(new LocalFile { Key = "index.html", ETag = "123" });
            LocalFiles.Add(new LocalFile { Key = "1/about.html", ETag = "567" });
            LocalFiles.Add(new LocalFile { Key = "sitemap.xml", ETag = "8664" });
            S3Objects = LocalFiles.Select(p => new S3Object { Key = p.Key, ETag = p.ETag }).ToList();
            S3Objects.Add(new S3Object { Key = "sitemap2.xml", ETag = "567" });
        };

        Because of = () => CompareResults = Comparer.CompareLocalToS3(LocalFiles, S3Objects);

        private It should_recognize_that_new_items_were_added =
            () => CompareResults.ShouldContainOnly(new CompareResultsBuilder()
                .Add("index.html", CompareStatus.Retained)
                .Add("1/about.html", CompareStatus.Retained)
                .Add("sitemap.xml", CompareStatus.Retained)
                .Add("sitemap2.xml", CompareStatus.Deleted).Build());
    }

    [Subject(typeof(ContentComparer))]
    public class when_items_are_added_retained_and_deleted
    {
        static ContentComparer Comparer;
        static List<LocalFile> LocalFiles = new List<LocalFile>();
        static List<S3Object> S3Objects = new List<S3Object>();
        static List<CompareResult> CompareResults = new List<CompareResult>();

        Establish context = () =>
        {
            Comparer = new ContentComparer();
            LocalFiles.Add(new LocalFile { Key = "indexadded.html", ETag = "123" });
            LocalFiles.Add(new LocalFile { Key = "retained.xml", ETag = "5566" });
            LocalFiles.Add(new LocalFile { Key = "retained2.xml", ETag = "5566" });
            LocalFiles.Add(new LocalFile { Key = "added.xml", ETag = "8944" });
            S3Objects.Add(new S3Object { Key = "retained2.xml", ETag = "5566" });
            S3Objects.Add(new S3Object { Key = "1/deleted.html", ETag = "567" });
            S3Objects.Add(new S3Object { Key = "deleted.xml", ETag = "58944" });
            S3Objects.Add(new S3Object { Key = "retained.xml", ETag = "5566" });
        };

        Because of = () => CompareResults = Comparer.CompareLocalToS3(LocalFiles, S3Objects);

        private It should_recognize_that_items_were_retained_added_and_deleted =
            () => CompareResults.ShouldContainOnly(new CompareResultsBuilder()
                .Add("indexadded.html", CompareStatus.Added)
                .Add("1/deleted.html", CompareStatus.Deleted)
                .Add("retained.xml", CompareStatus.Retained)
                .Add("retained2.xml", CompareStatus.Retained)
                .Add("added.xml", CompareStatus.Added)
                .Add("deleted.xml", CompareStatus.Deleted).Build());

    }
}