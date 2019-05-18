using System;
using System.Net.Http;
using System.Threading.Tasks;
using loc0CoreMatrixClient.Models;
using Newtonsoft.Json.Linq;

namespace loc0CoreMatrixClient
{
    /// <summary>
    /// Creates an instance of a matrix room allowing for interaction with said room
    /// </summary>
    public class MatrixRoom
    {
        private readonly MatrixHttp _backendHttpClient = new MatrixHttp();

        /// <summary>
        /// Room id for room
        /// </summary>
        public string RoomId { get; private set; }

        /// <summary>
        /// Room alias for room
        /// </summary>
        public string RoomAlias { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="roomAlias"></param>
        public MatrixRoom(string roomId = null, string roomAlias = null)
        {
            RoomId = roomId;
            RoomAlias = roomAlias;
        }

        private async Task<bool> SendMessageRequest(JObject jsonContent, string hostServer, string accessToken)
        {
            if (RoomId == null && RoomAlias == null)
                throw new NullReferenceException("Both RoomId and RoomAlias are null");

            if (string.IsNullOrWhiteSpace(RoomId))
            {
                HttpResponseMessage getRoomIdResponse = await _backendHttpClient.Get($"{hostServer}/_matrix/client/r0/directory/room/{RoomAlias}");

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
                $"{hostServer}/_matrix/client/r0/rooms/{RoomId}/send/m.room.message?access_token={accessToken}", jsonContent.ToString());

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
        /// <param name="hostServer">Host server or home server the room resides on</param>
        /// <param name="accessToken">Your clients access token</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixTextMessage textMessage, string hostServer, string accessToken)
        {
            var textJObject = new JObject
            {
                ["msgtype"] = "m.text",

                ["body"] = textMessage.Body ?? "",

                ["format"] = string.IsNullOrWhiteSpace(textMessage.FormattedBody) ? "" : "org.matrix.custom.html",

                ["formatted_body"] = textMessage.FormattedBody ?? ""
            };

            return await SendMessageRequest(textJObject, hostServer, accessToken);
        }

        /// <summary>
        /// Sends a file to the room
        /// </summary>
        /// <param name="fileMessage">MatrixFileMessage object that contains information for sending</param>
        /// <param name="hostServer">Host server or home server the room resides on</param>
        /// <param name="accessToken">Your clients access token</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixFileMessage fileMessage, string hostServer, string accessToken)
        {
            JObject jsonContent;

            if (fileMessage.Type == "m.file")
            {
                jsonContent = new JObject
                {
                    ["body"] = fileMessage.Filename ?? "",

                    ["filename"] = fileMessage.Filename ?? "",

                    ["info"] = new JObject(),

                    ["msgtype"] = "m.file",

                    ["url"] = fileMessage.MxcUrl ?? ""
                };
            }
            else
            {
                jsonContent = new JObject
                {
                    ["body"] = fileMessage.Filename ?? "",

                    ["info"] = new JObject(),

                    ["msgtype"] = fileMessage.Type ?? "",

                    ["url"] = fileMessage.MxcUrl ?? ""
                };
            }

            return await SendMessageRequest(jsonContent, hostServer, accessToken);
        }
    }
}