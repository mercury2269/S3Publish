using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using S3Publish.NetDna;

namespace S3Publish
{
    public class MaxCdnService
    {
        private static string _alias = ConfigurationManager.AppSettings["MaxCdn.Alias"];
        private static string _key = ConfigurationManager.AppSettings["MaxCdn.Key"];
        private static string _secret = ConfigurationManager.AppSettings["MaxCdn.Secret"];
        private static string _zone = ConfigurationManager.AppSettings["MaxCdn.Zone"];

        private MaxCdn _maxCdn;

        public MaxCdnService()
        {
            _maxCdn = new MaxCdn(_alias, _key, _secret);
        }

        public void PurgeFiles(List<string> keys)
        {
            foreach (var key in keys)
            {
                var file = key.StartsWith("/") ? key : "/" + key;
                _maxCdn.Delete("/zones/pull.json/" + _zone + "/cache?file=" + Uri.EscapeDataString(file));
                Console.WriteLine("Purged: " + file);
            }
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