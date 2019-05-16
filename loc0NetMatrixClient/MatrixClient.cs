using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using loc0NetMatrixClient.Events;
using loc0NetMatrixClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MimeTypes; //credit to samuelneff for mime types https://github.com/samuelneff/MimeTypeMap

namespace loc0NetMatrixClient
{
    /// <summary>
    /// Client for interacting with the Matrix API
    /// </summary>
    public class MatrixClient
    {
        private readonly MatrixHttp _backendHttpClient = new MatrixHttp();
        private readonly List<MatrixChannel> _activeRoomsList = new List<MatrixChannel>();
        private readonly CancellationTokenSource _syncCancellationToken = new CancellationTokenSource();
        private readonly int _messageLimit;
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

        /// <param name="messageLimit">Number of messages to take on each sync</param>
        public MatrixClient(int messageLimit = 10)
        {
            _messageLimit = messageLimit;
        }

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

            var loginResponse = await _backendHttpClient.Post($"{host}/_matrix/client/r0/login", loginJObject.ToString());

            try
            {
                loginResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return false;
            }

            var loginResponseJson = JsonConvert.DeserializeObject<LoginJson>(
                await loginResponse.Content.ReadAsStringAsync());

            AccessToken = loginResponseJson.AccessToken;
            DeviceId = loginResponseJson.DeviceId;
            HomeServer = loginResponseJson.HomeServer;

            if (!Regex.IsMatch(HomeServer, @"^https:\/\/"))
            {
                HomeServer = "https://" + HomeServer;
            }

            UserId = loginResponseJson.UserId;

            JObject filterJObject = new JObject
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

            var filterResponse =
                await _backendHttpClient.Post(filterUrl, filterJObject.ToString());

            try
            {
                filterResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Failed to upload filters\nAborting");
                Environment.Exit(1);
            }

            var filterResponseContent = await filterResponse.Content.ReadAsStringAsync();

            JObject filterJObjectParsed = JObject.Parse(filterResponseContent);

            _filterId = (string)filterJObjectParsed["filter_id"];

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
            List<string> responseList = new List<string>();

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
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                    responseList.Add($"Failed to Join {room}");

                    if (retryFailure)
                    {
                        i = i == 0 ? 0 : i - 1;
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
            List<string> response = await JoinRooms(new List<string>
            {
                roomToJoin
            }, retryFailure);

            return response[0];
        }

        /// <summary>
        /// Upload a file to Matrix
        /// </summary>
        /// <param name="fileDirectory">Path to the file you want to upload</param>
        /// <returns>mxcUri for later use</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<string> Upload(string fileDirectory)
        {
            if (!File.Exists(fileDirectory))
                throw new FileNotFoundException("File not found", Path.GetFileName(fileDirectory));

            var contentType = MimeTypeMap.GetMimeType(Path.GetExtension(fileDirectory));
            var filename = Path.GetFileNameWithoutExtension(fileDirectory);
            var fileBytes = File.ReadAllBytes(fileDirectory);

            HttpResponseMessage uploadResponse =
                await _backendHttpClient.Post($"{HomeServer}/_matrix/media/r0/upload?filename={filename}&access_token={AccessToken}", fileBytes,
                    contentType);

            try
            {
                uploadResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return "Failed to upload";
            }

            var uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();

            JObject uploadResponseJObject = JObject.Parse(uploadResponseContent);
            var mxcUrl = (string)uploadResponseJObject["content_uri"];

            return mxcUrl;
        }

        /// <summary>
        /// Starts a message listener for any rooms you've joined
        /// </summary>
        /// <returns></returns>
        public void StartListener() => Sync().ConfigureAwait(false);

        /// <summary>
        /// Stop listening for events in the rooms you've joined
        /// </summary>
        public void StopListener() => _syncCancellationToken.Cancel();

        /// <summary>
        /// Contact the sync endpoint
        /// </summary>
        /// <returns></returns>
        private async Task Sync() //break this up in the future, for now it's fine
        {
            HttpResponseMessage firstSyncResponse = await _backendHttpClient.Get(
                $"{HomeServer}/_matrix/client/r0/sync?filter={_filterId}&access_token={AccessToken}");

            try
            {
                firstSyncResponse.EnsureSuccessStatusCode(); //don't like this repeating code
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Sync failed");
                return;
            }

            string firstSyncResponseContents = await firstSyncResponse.Content.ReadAsStringAsync();

            JObject firstSyncJObject = JObject.Parse(firstSyncResponseContents);
            string nextBatch = (string)firstSyncJObject["next_batch"];

            await Task.Delay(2000);

            while (!_syncCancellationToken.IsCancellationRequested)
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
                    await Task.Delay(2000, _syncCancellationToken.Token);
                    continue;
                }

                string syncResponseMessageContents = await syncResponseMessage.Content.ReadAsStringAsync();

                JObject syncResponseJObject = JObject.Parse(syncResponseMessageContents);

                nextBatch = (string)syncResponseJObject["next_batch"];

                await Task.Run(() => SyncChecks(syncResponseJObject));

                await Task.Delay(2000, _syncCancellationToken.Token);
            }
        }

        private void SyncChecks(JObject syncJObject)
        {
            JToken roomJToken = syncJObject["rooms"]["join"];

            if (roomJToken.HasValues) //Process new messages
            {
                foreach (JToken room in roomJToken.Children())
                {
                    JProperty roomJProperty = (JProperty) room;
                    string roomId = roomJProperty.Name;

                    if (_activeRoomsList.All(x => x.ChannelId != roomId)) continue;

                    foreach (JToken message in room.First["timeline"]["events"].Children())
                    {
                        string sender = (string)message["sender"];
                        string body = (string)message["content"]["body"];

                        MessageReceivedEventArgs messageArgs = new MessageReceivedEventArgs(roomId, body, sender);
                        MessageReceived?.Invoke(messageArgs);
                    }
                }
            }

            JToken inviteJToken = syncJObject["rooms"]["invite"];

            if (inviteJToken.HasValues) //UNTESTED
            {
                foreach (var room in inviteJToken.Children())
                {
                    JProperty roomIdJProperty = (JProperty) room;
                    var roomId = roomIdJProperty.Name;

                    InviteReceivedEventArgs inviteArgs = new InviteReceivedEventArgs(roomId);
                    InviteReceived?.Invoke(inviteArgs);
                }
            }
        }
    }
}
