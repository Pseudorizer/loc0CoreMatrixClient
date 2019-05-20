using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using loc0CoreMatrixClient.Models;
using Newtonsoft.Json.Linq;

namespace loc0CoreMatrixClient
{
    /// <summary>
    /// Creates an instance of a matrix room allowing for interaction with said room
    /// </summary>
    public class MatrixRoom
    {
        private readonly MatrixHttp _backendHttpClient;

        /// <summary>
        /// Room id for room
        /// </summary>
        public string RoomId { get; private set; }

        /// <summary>
        /// Room alias for room
        /// </summary>
        public string RoomAlias { get; }

        /// <param name="accessToken">Current access token</param>
        /// <param name="roomId">Room ID I.E. !ID:Host</param>
        /// <param name="roomAlias">Room Alias I.E. #Name:Host</param>
        /// <param name="baseUrl">Home sever</param>
        public MatrixRoom(string baseUrl, string accessToken, string roomId = null, string roomAlias = null)
        {
            RoomId = roomId;
            RoomAlias = roomAlias;

            _backendHttpClient = new MatrixHttp(baseUrl, accessToken);
        }

        /// <summary>
        /// Set access token for httpClient in use by room
        /// </summary>
        /// <param name="token">Access token</param>
        public void SetAccessToken(string token) => _backendHttpClient.SetAccessToken(token);

        private async Task<bool> SendMessageRequest(JObject jsonContent)
        {
            if (RoomId == null && RoomAlias == null)
                throw new NullReferenceException("Both RoomId and RoomAlias are null");

            if (string.IsNullOrWhiteSpace(RoomId))
            {
                HttpResponseMessage getRoomIdResponse = await _backendHttpClient.Get($"/_matrix/client/r0/directory/room/{HttpUtility.UrlEncode(RoomAlias)}", false);

                try
                {
                    getRoomIdResponse.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Failed to get room ID from room alias");
                    return false;
                }

                var getRoomIdResponseContent = await getRoomIdResponse.Content.ReadAsStringAsync();

                JObject roomIdJObject = JObject.Parse(getRoomIdResponseContent);
                RoomId = (string) roomIdJObject["room_id"];
            }

            HttpResponseMessage sendResponse = await _backendHttpClient.Post(
                $"/_matrix/client/r0/rooms/{HttpUtility.UrlEncode(RoomId)}/send/m.room.message", true, jsonContent);

            try
            {
                sendResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Send a text message to the room
        /// </summary>
        /// <param name="textMessage">Message as a MatrixTextMessage</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixTextMessage textMessage)
        {
            textMessage.Format = "";
            var textJObject = JObject.FromObject(textMessage);

            textJObject["format"] = string.IsNullOrWhiteSpace(textMessage.FormattedBody) ? "" : "org.matrix.custom.html";

            return await SendMessageRequest(textJObject);
        }

        /// <summary>
        /// Sends a file to the room
        /// </summary>
        /// <param name="fileMessage">MatrixFileMessage object that contains information for sending</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixFileMessage fileMessage)
        {
            JObject jsonContent = JObject.FromObject(fileMessage);

            if (fileMessage.Type != "m.file")
            {
                jsonContent.Property("filename").Remove();
            }

            return await SendMessageRequest(jsonContent);
        }
    }
}