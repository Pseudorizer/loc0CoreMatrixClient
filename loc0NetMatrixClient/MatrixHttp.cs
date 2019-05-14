using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace loc0NetMatrixClient
{
    public class MatrixHttp
    {
        private readonly HttpClient _client = new HttpClient();

        public MatrixHttp()
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<HttpResponseMessage> Post(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));

            var response = await _client.SendAsync(request);

            return response;
        }
        
        public async Task<HttpResponseMessage> Post(string url, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            return response;
        }
    }
}