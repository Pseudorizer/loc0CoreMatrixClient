using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace loc0CoreMatrixClient
{
    /// <summary>
    /// Methods for communicating with the Matrix API
    /// </summary>
    internal class MatrixHttp : IDisposable
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client = new HttpClient();
        private string _accessToken;

        public MatrixHttp(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public MatrixHttp(string baseUrl, string accessToken)
        {
            _baseUrl = baseUrl;
            _accessToken = accessToken;
        }

        public void SetAccessToken(string accessToken) => _accessToken = accessToken;

        /// <summary>
        /// Wrapper for posting to a Matrix endpoint with content
        /// </summary>
        /// <param name="apiPath">Endpoint</param>
        /// <param name="authenticate">Whether you need to authenticate or not</param>
        /// <param name="data">Post data as a JObject</param>
        /// <param name="contentHeaders">Any content headers needed, format = HEADER:VALUE</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string apiPath, bool authenticate, JObject data, Dictionary<string, string> contentHeaders) //maybe i could use an out here? return a bool out a responsemessage
        {
            StringContent content = data != null 
                ? new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json") 
                : new StringContent("{}");

            foreach ((string key, string value) in contentHeaders)
            {
                content.Headers.Add(key, value);
            }

            string url = AddBaseAndToken(apiPath, authenticate);

            HttpResponseMessage response;

            try
            {
                response = await _client.PostAsync(new Uri(url), content);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is NullReferenceException)
                {
                    return null;
                }

                throw;
            }

            return response;
        }

        public async Task<HttpResponseMessage> Post(string apiPath, bool authenticate, byte[] data,
            Dictionary<string, string> contentHeaders)
        {
            using (ByteArrayContent content = data != null
                ? new ByteArrayContent(data)
                : new ByteArrayContent(new byte[0]))
            {
                foreach ((string key, string value) in contentHeaders)
                {
                    content.Headers.Add(key, value);
                }

                string url = AddBaseAndToken(apiPath, authenticate);

                HttpResponseMessage response;

                try
                {
                    response = await _client.PostAsync(new Uri(url), content);
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException || ex is TaskCanceledException)
                    {
                        return null;
                    }

                    throw;
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> Post(string apiPath, bool authenticate, JObject data)
        {
            return await Post(apiPath, authenticate, data, new Dictionary<string, string>());
        }

        /// <summary>
        /// Wrapper for getting from a Matrix endpoint
        /// </summary>
        /// <param name="apiPath">Endpoint</param>
        /// <param name="authenticate"></param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Get(string apiPath, bool authenticate)
        {
            HttpResponseMessage response;

            try
            {
                response = await _client.GetAsync(new Uri(AddBaseAndToken(apiPath, authenticate)));
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    return null;
                }

                throw;
            }

            return response;
        }

        /// <summary>
        /// Wrapper for putting from a Matrix endpoint
        /// </summary>
        /// <param name="apiPath">Endpoint</param>
        /// <param name="content">Content to be posted</param>
        /// <param name="contentType">Content type, defaults to application/json</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Put(string apiPath, string content, string contentType = "application/json") //update to POST style when needed
        {
            HttpResponseMessage response;

            using (var messageContent = new StringContent(content, Encoding.UTF8, contentType))
            {
                response = await _client.PutAsync(new Uri(AddHttps(apiPath)), messageContent);
            }

            return response;
        }

        private string AddBaseAndToken(string apiPath, bool authenticate)
        {
            apiPath = AddHttps(_baseUrl) + apiPath;

            if (authenticate)
            {
                apiPath += apiPath.Contains("?") ? "&" : "?";
                apiPath += "access_token=" + _accessToken;
            }

            return apiPath;
        }

        private static string AddHttps(string url) => url.StartsWith("https://") ? url : "https://" + url;

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}