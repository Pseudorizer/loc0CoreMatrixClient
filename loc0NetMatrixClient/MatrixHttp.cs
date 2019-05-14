using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace loc0NetMatrixClient
{
    /// <summary>
    /// Methods for communicating with the Matrix API
    /// </summary>
    internal class MatrixHttp
    {
        private readonly HttpClient _client = new HttpClient();

        public MatrixHttp()
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        /// <summary>
        /// Wrapper for posting to a Matrix endpoint without content
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url)))
            {
                return await _client.SendAsync(request);
            }
        }
        
        /// <summary>
        /// Wrapper for posting to a Matrix endpoint with content
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <param name="content">Content to be posted</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Post(string url, string content)
        {
            HttpResponseMessage response;

            using (var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = new StringContent(content, Encoding.UTF8, "application/json")})
            {
                response = await _client.SendAsync(request);
            }

            return response;
        }

        /// <summary>
        /// Wrapper for getting from a Matrix endpoint
        /// </summary>
        /// <param name="url">Endpoint</param>
        /// <returns>HttpResponseMessage for consumption</returns>
        public async Task<HttpResponseMessage> Get(string url) => await _client.GetAsync(new Uri(url));
    }
}