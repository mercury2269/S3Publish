﻿using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace S3Publish.NetDna
{
    public class MaxCdn
    {
        private string _consumerKey = "";
        private string _consumerSecret = "";
        private string _alias = "";
        private int _requestTimeout = 30;

        private const string _netDNABaseAddress = "https://rws.netdna.com";

        public MaxCdn(string alias, string consumerKey, string consumerSecret, int requestTimeout = 30)
        {
            _consumerKey = consumerKey;
            _alias = alias;
            _consumerSecret = consumerSecret;
            _requestTimeout = requestTimeout;
        }

        public dynamic Get(string url, bool debug = false)
        {            
            var requestUrl = GenerateOAuthRequestUrl(url, "GET");

            var request = new ApiWebClient(_requestTimeout);            
            var response = request.DownloadString(requestUrl);

            var result = JObject.Parse(response);

            if (debug)
                DumpObject(result);

            return result;
        }

        public bool Delete(string url)
        {
            var response = GetWebResponse(url, "DELETE");
            return ((HttpWebResponse) response).StatusCode == HttpStatusCode.OK;
        }

        private WebResponse GetWebResponse(string url, string method)
        {
            var requestUrl = GenerateOAuthRequestUrl(url, method);

            var request = WebRequest.Create(requestUrl);
            request.Method = method;

            var response = request.GetResponse();
            return response;
        }

        public bool Put(string url, dynamic data)
        {
            var requestUrl = GenerateOAuthRequestUrl(url, "PUT");

            var request = WebRequest.Create(requestUrl);
            request.Method = "PUT";

            var jsonData = JsonConvert.SerializeObject(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();

            return ((HttpWebResponse)response).StatusCode == HttpStatusCode.OK;            
        }

        private string GenerateOAuthRequestUrl(string url, string method)
        {
            Uri uri;
            Uri.TryCreate(_netDNABaseAddress + "/" + _alias + url, UriKind.Absolute, out uri);

            var normalizedUrl = "";
            var normalizedParams = "";

            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();
            var sig =
                HttpUtility.UrlEncode(oAuth.GenerateSignature(uri, _consumerKey, _consumerSecret, "", "", method, timeStamp,
                                                              nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl,
                                                              out normalizedParams));
            var requestUrl = normalizedUrl + "?" + normalizedParams + "&oauth_signature=" + sig;
            return requestUrl;
        }

        private void DumpObject(dynamic o)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(o))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(o);
                Console.Write("{0}={1} ", name, value);
            }

            Console.WriteLine();
        }

        private class ApiWebClient : WebClient
        {
            private int _requestTimeout = 30;

            public ApiWebClient(int requestTimeout = 30)
            {
                _requestTimeout = requestTimeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest w = base.GetWebRequest(address);
                w.Timeout = _requestTimeout * 1000;
                return w;
            }
        }
    }
}
