using System.Net.Http;
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
        private readonly string _roomId;

        /// <param name="roomId">ID of room you want to call</param>
        public MatrixRoom(string roomId)
        {
            _roomId = HttpUtility.UrlEncode(roomId);
        }

        /// <summary>
        /// Send a textMessage to the room
        /// </summary>
        /// <param name="textMessage">Message as a MatrixTextMessage</param>
        /// <param name="hostServer">Host server or home server the room resides on</param>
        /// <param name="accessToken">Your clients access token</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixTextMessage textMessage, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["msgtype"] = textMessage.Type ?? "",

                ["body"] = textMessage.Body ?? "",

                ["format"] = textMessage.Format ?? "",

                ["formatted_body"] = textMessage.FormattedBody ?? ""
            };

            if (!Regex.IsMatch(hostServer, @"^https:\/\/"))
            {
                hostServer = "https://" + hostServer;
            }

            var sendResponse = await _backendHttpClient.Post(
                $"{hostServer}/_matrix/client/r0/rooms/{_roomId}/send/m.room.message?access_token={accessToken}", messageJObject.ToString());

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
        /// Send an image file via a mxcUri
        /// </summary>
        /// <param name="matrixFileUrl">mxcUri of uploaded content</param>
        /// <param name="filename">Filename people will see in chat, doesn't actually change the filename when downloading</param>
        /// <param name="hostServer"></param>
        /// <param name="accessToken"></param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendImage(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.image",

                ["url"] = matrixFileUrl
            };

            var sendImageResponse = await _backendHttpClient.Post(
                $"{hostServer}/_matrix/client/r0/rooms/{_roomId}/send/m.room.message?access_token={accessToken}",
                messageJObject.ToString());

            try
            {
                sendImageResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Send an audio file via a mxcUri
        /// </summary>
        /// <inheritdoc cref="SendImage"/>
        public async Task<bool> SendAudio(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.audio",

                ["url"] = matrixFileUrl
            };

            var sendAudioResponse = await _backendHttpClient.Post(
                $"{hostServer}/_matrix/client/r0/rooms/{_roomId}/send/m.room.message?access_token={accessToken}",
                messageJObject.ToString());

            try
            {
                sendAudioResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> SendVideo(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["body"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.video",

                ["url"] = matrixFileUrl
            };

            var sendVideoResponse = await _backendHttpClient.Post(
                $"{hostServer}/_matrix/client/r0/rooms/{_roomId}/send/m.room.message?access_token={accessToken}",
                messageJObject.ToString());

            try
            {
                sendVideoResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Send a file via a mxcUri
        /// </summary>
        /// <inheritdoc cref="SendImage"/>
        public async Task<bool> SendFile(string matrixFileUrl, string filename, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["body"] = filename,

                ["filename"] = filename,

                ["info"] = new JObject(),

                ["msgtype"] = "m.file",

                ["url"] = matrixFileUrl
            };

            var sendFileResponse = await _backendHttpClient.Post(
                $"{hostServer}/_matrix/client/r0/rooms/{_roomId}/send/m.room.message?access_token={accessToken}",
                messageJObject.ToString());

            try
            {
                sendFileResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}