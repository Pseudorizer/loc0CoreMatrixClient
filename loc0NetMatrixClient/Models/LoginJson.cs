using Newtonsoft.Json;

namespace loc0NetMatrixClient.Models
{
    /// <summary>
    /// JSON structure for login response
    /// </summary>
    internal class LoginJson
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("home_server")]
        public string HomeServer { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
    }
}