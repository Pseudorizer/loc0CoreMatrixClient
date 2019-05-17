using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using loc0NetMatrixClient.Models;
using Newtonsoft.Json.Linq;

namespace loc0NetMatrixClient
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


        /// <param name="roomId">ID of room you want to call</param>
        /// <param name="roomAlias">Alias of room you want to call</param>
        public MatrixRoom(string roomId = null, string roomAlias = null)
        {
            if (roomId == null && roomAlias == null)
                throw new ArgumentException("Both roomId and roomAlias cannot be left empty");

            RoomAlias = HttpUtility.UrlEncode(roomAlias) ?? "";
            RoomId = HttpUtility.UrlEncode(roomId) ?? "";
        }

        private async Task<bool> SendMessage(JObject jsonContent, string hostServer, string accessToken)
        {
            if (!Regex.IsMatch(hostServer, @"^https:\/\/"))
            {
                hostServer = "https://" + hostServer;
            }

            if (string.IsNullOrWhiteSpace(RoomId))
            {
                var getRoomIdResponse = await _backendHttpClient.Get($"{hostServer}/_matrix/client/r0/directory/room/{RoomAlias}");
                string getRoomIdResponseContent = await getRoomIdResponse.Content.ReadAsStringAsync();

                JObject roomIdJObject = JObject.Parse(getRoomIdResponseContent);
                RoomId = HttpUtility.UrlEncode((string) roomIdJObject["room_id"]);
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
        /// Send a textMessage to the room
        /// </summary>
        /// <param name="textMessage">Message as a MatrixTextMessage</param>
        /// <param name="hostServer">Host server or home server the room resides on</param>
        /// <param name="accessToken">Your clients access token</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendText(MatrixTextMessage textMessage, string hostServer, string accessToken)
        {
            var textJObject = new JObject
            {
                ["msgtype"] = textMessage.Type ?? "",

                ["body"] = textMessage.Body ?? "",

                ["format"] = textMessage.Format ?? "",

                ["formatted_body"] = textMessage.FormattedBody ?? ""
            };

            return await SendMessage(textJObject, hostServer, accessToken);
        }

        /// <summary>
        /// Send an image file via a mxcUri
        /// </summary>
        /// <param name="matrixFileUrl">mxcUri of uploaded content</param>
        /// <param name="filename">Filename people will see in chat, doesn't actually change the filename when downloading</param>
        /// <param name="hostServer"></param>
        /// <param name="accessToken"></param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendImage(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            var imageJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.image",

                ["url"] = matrixFileUrl
            };

            return await SendMessage(imageJObject, hostServer, accessToken);
        }

        /// <summary>
        /// Send an audio file via a mxcUri
        /// </summary>
        /// <inheritdoc cref="SendImage"/>
        public async Task<bool> SendAudio(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            var audioJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.audio",

                ["url"] = matrixFileUrl
            };

            return await SendMessage(audioJObject, hostServer, accessToken);
        }

        /// <summary>
        /// Send a video file via a mxcUri
        /// </summary>
        /// <inheritdoc cref="SendImage"/>
        public async Task<bool> SendVideo(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            var videoJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.video",

                ["url"] = matrixFileUrl
            };

            return await SendMessage(videoJObject, hostServer, accessToken);
        }

        /// <summary>
        /// Send a file via a mxcUri
        /// </summary>
        /// <inheritdoc cref="SendImage"/>
        public async Task<bool> SendFile(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            var fileJObject = new JObject
            {
                ["body"] = filename,

                ["filename"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.file",

                ["url"] = matrixFileUrl
            };

            return await SendMessage(fileJObject, hostServer, accessToken);
        }
    }
}