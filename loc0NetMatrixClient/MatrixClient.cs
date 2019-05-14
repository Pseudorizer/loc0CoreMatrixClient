using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace loc0NetMatrixClient
{
    /// <summary>
    /// Client for interacting with the Matrix API
    /// </summary>
    public class MatrixClient
    {
        private readonly MatrixHttp _backendHttpClient = new MatrixHttp();
        /// <summary>
        /// AccessToken to be used when interacting with the API
        /// </summary>
        public string AccessToken { get; private set; }
        /// <summary>
        /// DeviceId in use
        /// </summary>
        public string DeviceId { get; private set; }
        /// <summary>
        /// Homeserver the client is connected to
        /// </summary>
        public string HomeServer { get; private set; }
        /// <summary>
        /// Full userid of account
        /// </summary>
        public string UserId { get; private set; }
        
        /// <summary>
        /// Login to a Matrix account
        /// </summary>
        /// <param name="host">Homeserver the account uses</param>
        /// <param name="credentials">MatrixCredentials object that contains login info</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> Login(string host, MatrixCredentials credentials)
        {
            var loginJson = new JObject(
                new JProperty("type", "m.login.password"),
                new JProperty("identifier",
                    new JObject(
                        new JProperty("type", "m.id.user"),
                        new JProperty("user", credentials.UserName))),
                new JProperty("password", credentials.Password),
                new JProperty("initial_device_display_name", credentials.UserName),
                new JProperty("device_id", credentials.DeviceId));

            var loginResponse = await _backendHttpClient.Post(host + "/_matrix/client/r0/login", loginJson.ToString());

            try
            {
                loginResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            var loginResponseJson = JsonConvert.DeserializeObject<MatrixLoginResponse>(
                await loginResponse.Content.ReadAsStringAsync());

            AccessToken = loginResponseJson.AccessToken;
            DeviceId = loginResponseJson.DeviceId;
            HomeServer = "https://" + loginResponseJson.HomeServer;
            UserId = loginResponseJson.UserId;

            return true;
        }

        /// <summary>
        /// Join multiple rooms via a list
        /// </summary>
        /// <param name="roomsToJoin">List of rooms to join, can be alias or id</param>
        /// <param name="retryFailure">If connecting to a room fails, it will retry until success</param>
        /// <returns>List of strings denoting failure or success</returns>
        public async Task<List<string>> JoinRooms(List<string> roomsToJoin, bool retryFailure = false)
        {
            List<string> responseList = new List<string>();
            
            for (var i = 0; i < roomsToJoin.Count; i++)
            {
                var room = roomsToJoin[i];
                Console.WriteLine($"Joining {room}");
                
                var requestUrl = HomeServer + "/_matrix/client/r0/join/" + HttpUtility.UrlEncode(room) +
                                 "?access_token=" + AccessToken;

                var roomResponse = await _backendHttpClient.Post(requestUrl);

                try
                {
                    roomResponse.EnsureSuccessStatusCode();
                    responseList.Add($"Successfully Joined {room}");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                    responseList.Add($"Failed to Join {room}");

                    if (retryFailure)
                    {
                        i = i == 0 ? 0 : i - 1;
                    }
                }
            }

            return responseList;
        }
        
        /// <summary>
        /// Join a single room
        /// </summary>
        /// <param name="roomToJoin">Room to join, can be alias or id</param>
        /// <returns>String denoting failure or success</returns>
        public async Task<string> JoinRoom(string roomToJoin)
        {
            var response = await JoinRooms(new List<string>
            {
                roomToJoin
            });

            return response[0];
        }

        public delegate void MessageHandler(object obj, MessageReceivedEventArgs args);
        
        /// <summary>
        /// JSON structure for login response
        /// </summary>
        private class MatrixLoginResponse
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
}