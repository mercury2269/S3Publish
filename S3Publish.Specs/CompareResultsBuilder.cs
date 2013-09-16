using System.Collections.Generic;

namespace S3Publish.Specs
{
    public class CompareResultsBuilder
    {
        private List<CompareResult> _results;

        public CompareResultsBuilder()
        {
            _results = new List<CompareResult>();
        }

        public CompareResultsBuilder Add(string key, CompareStatus status)
        {
            _results.Add(new CompareResult(key, status));
            return this;
        }

        public List<CompareResult> Build()
        {
            return _results;
        } 
    }
}