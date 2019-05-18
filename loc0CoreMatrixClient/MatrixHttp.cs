using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace loc0CoreMatrixClient
{
    /// <summary>
    /// Methods for communicating with the Matrix API
    /// </summary>
    internal class MatrixHttp
    {
        private readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Wrapper for posting to a Matrix endpoint without content
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string url)
        {
            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(AddHttps(url))))
            {
                return await _client.SendAsync(request);
            }
        }

        /// <summary>
        /// Wrapper for posting to a Matrix endpoint with content
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <param name="content">Content to be posted</param>
        /// <param name="contentType">Content type, defaults to application/json</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string url, string content, string contentType = "application/json")
        {
            HttpResponseMessage response;

            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            using (var request = new StringContent(content, Encoding.UTF8, contentType))
            {
                response = await _client.PostAsync(new Uri(AddHttps(url)), request);
            }

            return response;
        }

        /// <summary>
        /// Wrapper for posting to a Matrix endpoint with a byte[]
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <param name="content">Content as a byte[] to be posted</param>
        /// <param name="contentType">Content type</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string url, byte[] content, string contentType)
        {
            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            var byteArrayContent = new ByteArrayContent(content);
            byteArrayContent.Headers.Add("Content-Type", contentType);
            HttpResponseMessage response = await _client.PostAsync(new Uri(AddHttps(url)), byteArrayContent);

            return response;
        }

        /// <summary>
        /// Wrapper for getting from a Matrix endpoint
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Get(string url) => await _client.GetAsync(new Uri(AddHttps(url)));

        /// <summary>
        /// Wrapper for putting from a Matrix endpoint
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <param name="content">Content to be posted</param>
        /// <param name="contentType">Content type, defaults to application/json</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Put(string url, string content, string contentType = "application/json")
        {
            HttpResponseMessage response;

            using (var messageContent = new StringContent(content, Encoding.UTF8, contentType))
            {
                response = await _client.PutAsync(new Uri(AddHttps(url)), messageContent);
            }

            return response;
        }

        private static string AddHttps(string url) => url.StartsWith("https://") ? url : "https://" + url;
    }
}