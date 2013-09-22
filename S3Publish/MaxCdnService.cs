using System.Configuration;
using S3Publish.NetDna;

namespace S3Publish
{
    public class MaxCdnService
    {
        private static string _alias = ConfigurationManager.AppSettings["MaxCdn.Alias"];
        private static string _key = ConfigurationManager.AppSettings["MaxCdn.Key"];
        private static string _secret = ConfigurationManager.AppSettings["MaxCdn.Secret"];

        private MaxCdn _maxCdn;

        public MaxCdnService()
        {
            _maxCdn = new MaxCdn(_alias, _key, _secret);
        }

        public void PurgeCache()
        {
            var pullZones = _maxCdn.Get("/zones/pull.json");
            foreach (var pullZone in pullZones.data.pullzones)
            {
                _maxCdn.Delete("/zones/pull.json/" + pullZone.id + "/cache");
            }
        }
    }
}