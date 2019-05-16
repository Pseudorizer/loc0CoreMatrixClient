using System;
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
        /// Send a message to the room
        /// </summary>
        /// <param name="message">Message as a MatrixMessage</param>
        /// <param name="hostServer">Host server or home server the room resides on</param>
        /// <param name="accessToken">Your clients access token</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> SendMessage(MatrixMessage message, string hostServer, string accessToken)
        {
            JObject messageJObject = new JObject
            {
                ["msgtype"] = message.Type ?? "",

                ["body"] = message.Body ?? "",

                ["format"] = message.Format ?? "",

                ["formatted_body"] = message.FormattedBody ?? ""
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
        /// Send a matrix file via a mxcUri
        /// </summary>
        /// <param name="matrixFileUrl">mxcUri of uploaded content</param>
        /// <param name="filename"></param>
        /// <param name="hostServer"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
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

        private async Task GetFileInfo(string url, string hostServer, string accessToken)
        {
            url = url.Replace("mxc://", "https://");

            var p =
                $"{hostServer}/_matrix/media/r0/preview_url?url={HttpUtility.UrlEncode(url)}&access_token={accessToken}";
            var fileInfoResponse = await _backendHttpClient.Get(
                $"{hostServer}/_matrix/media/r0/preview_url?url={HttpUtility.UrlEncode("https://matrix.org/docs/spec/client_server/r0.4.0.html#get-matrix-client-r0-sync")}&access_token={accessToken}");

            try
            {
                fileInfoResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var fileInfoResponseContent = await fileInfoResponse.Content.ReadAsStringAsync();

            JObject r = JObject.Parse(fileInfoResponseContent);
            var q = r.ToString();
        }
    }
}