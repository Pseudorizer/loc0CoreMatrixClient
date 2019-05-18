using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using loc0CoreMatrixClient.Events;
using loc0CoreMatrixClient.Models;
using MimeTypes;
using Newtonsoft.Json.Linq;

namespace loc0CoreMatrixClient
{
    /// <summary>
    /// Client for interacting with the Matrix API
    /// </summary>
    public class MatrixClient
    {
        private readonly MatrixHttp _backendHttpClient = new MatrixHttp();
        private readonly List<MatrixChannel> _activeRoomsList = new List<MatrixChannel>();
        private readonly int _messageLimit;
        private CancellationTokenSource _syncCancellationToken;
        private bool _syncActive;
        private string _filterId;

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

        /// <inheritdoc />
        public delegate void MessageReceivedEvent(MessageReceivedEventArgs args);

        /// <inheritdoc />
        public delegate void InviteReceivedEvent(InviteReceivedEventArgs args);

        /// <summary>
        /// Event for any incoming messages
        /// </summary>
        public event MessageReceivedEvent MessageReceived;

        /// <summary>
        /// Event for any incoming invites
        /// </summary>
        public event InviteReceivedEvent InviteReceived;

        /// <param name="messageLimit">Number of messages to take on each sync</param>
        public MatrixClient(int messageLimit = 10)
        {
            _messageLimit = messageLimit;
        }

        /// <summary>
        /// Login to a Matrix account
        /// </summary>
        /// <param name="host">Homeserver the account uses</param>
        /// <param name="credentials">MatrixCredentials object that contains login info</param>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> Login(string host, MatrixCredentials credentials)
        {
            JObject loginJObject = new JObject
            {
                ["type"] = "m.login.password",

                ["identifier"] = new JObject
                {
                    ["type"] = "m.id.user",
                    ["user"] = credentials.UserName ?? ""
                },

                ["password"] = credentials.Password ?? "",

                ["initial_device_display_name"] = credentials.DeviceName ?? "",

                ["device_id"] = credentials.DeviceId ?? ""
            };

            HttpResponseMessage loginResponse = await _backendHttpClient.Post($"{host}/_matrix/client/r0/login", loginJObject.ToString());

            try
            {
                loginResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return false;
            }

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

            var loginResponseJObject = JObject.Parse(loginResponseContent);

            AccessToken = (string)loginResponseJObject["access_token"];
            DeviceId = (string)loginResponseJObject["device_id"];
            HomeServer = (string)loginResponseJObject["home_server"];
            UserId = (string)loginResponseJObject["user_id"];

            if (!Regex.IsMatch(HomeServer, @"^https:\/\/"))
            {
                HomeServer = "https://" + HomeServer;
            }

            var filtersResult = await UploadFilters();

            if (!filtersResult)
            {
                Console.WriteLine("Failed to upload filters");
            }

            return true;
        }

        private async Task<bool> UploadFilters()
        {
            var filterJObject = new JObject
            {
                ["room"] = new JObject
                {
                    ["state"] = new JObject
                    {
                        ["not_types"] = new JArray("*")
                    },

                    ["timeline"] = new JObject
                    {
                        ["limit"] = _messageLimit,
                        ["types"] = new JArray("m.room.message")
                    },

                    ["ephemeral"] = new JObject
                    {
                        ["not_types"] = new JArray("*")
                    }
                },

                ["presence"] = new JObject
                {
                    ["not_types"] = new JArray("*")
                },

                ["event_format"] = "client",

                ["event_fields"] = new JArray("content", "sender")
            };

            var filterUrl = $"{HomeServer}/_matrix/client/r0/user/{UserId}/filter?access_token={AccessToken}";

            HttpResponseMessage filterResponse =
                await _backendHttpClient.Post(filterUrl, filterJObject.ToString());

            try
            {
                filterResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return false;
            }

            var filterResponseContent = await filterResponse.Content.ReadAsStringAsync();
            JObject filterJObjectParsed = JObject.Parse(filterResponseContent);
            _filterId = (string)filterJObjectParsed["filter_id"];

            return true;
        }

        /// <summary>
        /// Logout of your current account, resets all properties
        /// </summary>
        /// <returns>Bool based on success or failure</returns>
        public async Task<bool> Logout()
        {
            HttpResponseMessage logoutResponse = await _backendHttpClient.Post($"{HomeServer}/_matrix/client/r0/logout?access_token={AccessToken}");

            try
            {
                logoutResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return false;
            }

            AccessToken = "";
            DeviceId = "";
            UserId = "";
            HomeServer = "";

            return true;
        }

        /// <summary>
        /// Join multiple rooms via a list
        /// </summary>
        /// <param name="roomsToJoin">List of rooms to join, can be alias or id</param>
        /// <param name="retryFailure">If connecting to a room fails, it will retry until success</param>
        /// <returns>List of strings denoting failure or success</returns>
        public async Task<List<string>> JoinRooms(List<string> roomsToJoin, bool retryFailure = false) //replace List<string> with something else, i don't know what yet
        {
            var responseList = new List<string>();

            for (var i = 0; i < roomsToJoin.Count; i++)
            {
                var room = roomsToJoin[i];
                Console.WriteLine($"Joining {room}");

                var requestUrl =
                    $"{HomeServer}/_matrix/client/r0/join/{HttpUtility.UrlEncode(room)}?access_token={AccessToken}";

                HttpResponseMessage roomResponse = await _backendHttpClient.Post(requestUrl);

                try
                {
                    roomResponse.EnsureSuccessStatusCode();
                    responseList.Add($"Successfully Joined {room}");
                    JObject roomResponseJObject = JObject.Parse(await roomResponse.Content.ReadAsStringAsync());
                    _activeRoomsList.Add(new MatrixChannel((string)roomResponseJObject["room_id"]));
                }
                catch (HttpRequestException)
                {
                    if (retryFailure)
                    {
                        i = i == 0 ? 0 : i - 1;
                    }
                    else
                    {
                        responseList.Add($"Failed to Join {room}");
                    }
                }

                await Task.Delay(2000);
            }

            return responseList;
        }

        /// <summary>
        /// Join a single room
        /// </summary>
        /// <param name="roomToJoin">Room to join, can be alias or id</param>
        /// <param name="retryFailure">If connecting to a room fails, it will retry until success</param>
        /// <returns>String denoting failure or success</returns>
        public async Task<string> JoinRoom(string roomToJoin, bool retryFailure = false)
        {
            var response = await JoinRooms(new List<string>
            {
                roomToJoin
            }, retryFailure);

            return response[0];
        }

        /// <summary>
        /// Upload a file to Matrix
        /// </summary>
        /// <param name="filePath">Path to the file you want to upload</param>
        /// <returns>MatrixFileMessage with MxcUrl and Type</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<MatrixFileMessage> Upload(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", Path.GetFileName(filePath));

            var contentType = MimeTypeMap.GetMimeType(Path.GetExtension(filePath)); //credit to samuelneff for mime types https://github.com/samuelneff/MimeTypeMap
            var filename = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);

            HttpResponseMessage uploadResponse =
                await _backendHttpClient.Post($"{HomeServer}/_matrix/media/r0/upload?filename={filename}&access_token={AccessToken}", fileBytes,
                    contentType);

            try
            {
                uploadResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return null;
            }

            var uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();

            JObject uploadResponseJObject = JObject.Parse(uploadResponseContent);

            MatrixFileMessage matrixFileMessage = new MatrixFileMessage
            {
                MxcUrl = (string)uploadResponseJObject["content_uri"]
            };

            var contentTypeSplit = contentType.Split("/")[0];

            switch (contentTypeSplit)
            {
                case "image":
                case "video":
                case "audio":
                    matrixFileMessage.Type = $"m.{contentTypeSplit}";
                    break;
                default:
                    matrixFileMessage.Type = "m.file";
                    break;
            }

            matrixFileMessage.Filename = filename;

            return matrixFileMessage;
        }

        /// <summary>
        /// Exception for a user attempting to start a second listener thread
        /// </summary>
        public class SyncAlreadyActiveException : Exception
        {
            /// <inheritdoc />
            public SyncAlreadyActiveException()
            {
            }

            /// <inheritdoc />
            /// <param name="message">Message to display</param>
            public SyncAlreadyActiveException(string message)
            : base(message)
            {
            }
        }

        /// <summary>
        /// Starts an event listener for any rooms you've joined
        /// </summary>
        /// <returns></returns>
        public void StartListener()
        {
            if (_syncActive)
                throw new SyncAlreadyActiveException("You can't start another listener");

            _syncActive = true;
            _syncCancellationToken = new CancellationTokenSource();
            Sync(_syncCancellationToken.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Stop listening for events in the rooms you've joined
        /// </summary>
        public void StopListener()
        {
            if (_syncCancellationToken != null)
            {
                _syncCancellationToken.Cancel();
                _syncActive = false;
            }
        }

        /// <summary>
        /// Contact the sync endpoint in a fire and forget background task
        /// </summary>
        private async Task Sync(CancellationToken syncCancellationToken)
        {
            var nextBatch = string.Empty;

            while (string.IsNullOrWhiteSpace(nextBatch) && !syncCancellationToken.IsCancellationRequested)
            {
                nextBatch = await FirstSync();
                await Task.Delay(2000, syncCancellationToken);
            }

            while (!syncCancellationToken.IsCancellationRequested)
            {
                HttpResponseMessage syncResponseMessage = await _backendHttpClient.Get(
                    $"{HomeServer}/_matrix/client/r0/sync?filter={_filterId}&since={nextBatch}&access_token={AccessToken}");
                try
                {
                    syncResponseMessage.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Sync failed");
                    await Task.Delay(2000, syncCancellationToken);
                    continue;
                }

                var syncResponseMessageContents = await syncResponseMessage.Content.ReadAsStringAsync();

                JObject syncResponseJObject = JObject.Parse(syncResponseMessageContents);

                nextBatch = (string)syncResponseJObject["next_batch"];

                SyncChecks(syncResponseJObject);

                await Task.Delay(2000, syncCancellationToken);
            }
        }

        private async Task<string> FirstSync()
        {
            HttpResponseMessage firstSyncResponse = await _backendHttpClient.Get(
                $"{HomeServer}/_matrix/client/r0/sync?filter={_filterId}&access_token={AccessToken}");

            try
            {
                firstSyncResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Initial Sync failed");
                return null;
            }

            var firstSyncResponseContents = await firstSyncResponse.Content.ReadAsStringAsync();

            JObject firstSyncJObject = JObject.Parse(firstSyncResponseContents);
            return (string)firstSyncJObject["next_batch"];
        }

        private void SyncChecks(JObject syncJObject)
        {
            JToken roomJToken = syncJObject["rooms"]["join"];

            if (roomJToken.HasValues) //Process new messages
            {
                foreach (JToken room in roomJToken.Children())
                {
                    var roomJProperty = (JProperty)room;
                    var roomId = roomJProperty.Name;

                    if (_activeRoomsList.All(x => x.ChannelId != roomId)) continue;

                    foreach (JToken message in room.First["timeline"]["events"].Children())
                    {
                        var sender = (string)message["sender"];
                        var body = (string)message["content"]["body"];

                        var messageArgs = new MessageReceivedEventArgs(roomId, body, sender);
                        MessageReceived?.Invoke(messageArgs);
                    }
                }
            }

            JToken inviteJToken = syncJObject["rooms"]["invite"];

            if (inviteJToken.HasValues) //UNTESTED
            {
                foreach (JToken room in inviteJToken.Children())
                {
                    var roomIdJProperty = (JProperty)room;
                    string roomId = roomIdJProperty.Name;

                    var inviteArgs = new InviteReceivedEventArgs(roomId);
                    InviteReceived?.Invoke(inviteArgs);
                }
            }
        }
    }
}
