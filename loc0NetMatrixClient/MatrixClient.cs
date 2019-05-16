using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using loc0NetMatrixClient.Events;
using loc0NetMatrixClient.Models;
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
        private readonly List<MatrixChannel> _activeRoomsList = new List<MatrixChannel>();
        private readonly CancellationTokenSource _syncCancellationToken = new CancellationTokenSource();
        private readonly int _messageLimit;
        private string _filterId;
        private string _filterString;

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

        /// <summary>
        /// Event for any incoming messages
        /// </summary>
        public event MessageReceivedEvent MessageReceived;

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

            var loginResponse = await _backendHttpClient.Post($"{host}/_matrix/client/r0/login", loginJson.ToString());

            try
            {
                loginResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
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

            JObject filterJObject = new JObject(
                new JProperty("room",
                    new JObject(
                        new JProperty("state",
                            new JObject(
                                new JProperty("not_types",
                                    new JArray("*")))),
                        new JProperty("timeline",
                            new JObject(
                                new JProperty("limit", _messageLimit),
                                new JProperty("types",
                                    new JArray("m.room.message")))),
                        new JProperty("ephemeral",
                            new JObject(
                                new JProperty("not_types",
                                    new JArray("*")))))),
                new JProperty("presence",
                    new JObject(
                        new JProperty("not_types",
                            new JArray("*")))),
                new JProperty("event_format", "client"),
                new JProperty("event_fields",
                    new JArray("content"))); //replace with an object at some point, not easy to understand

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
            _filterString = filterJObject.ToString();

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

                var roomResponse = await _backendHttpClient.Post(requestUrl);

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
            var response = await JoinRooms(new List<string>
            {
                roomToJoin
            }, retryFailure);

            return response[0];
        }

        /// <summary>
        /// Upload a file to Matrix
        /// </summary>
        /// <param name="fileDirectory">Path to the file you want to upload</param>
        /// <param name="contentType">Content type, I.E. image/png</param>
        /// <returns>mxcUri for later use</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> Upload(string fileDirectory, string contentType)
        {
            if (!File.Exists(fileDirectory))
                throw new ArgumentException("File not found", nameof(fileDirectory));

            var filename = Path.GetFileNameWithoutExtension(fileDirectory);
            var fileBytes = File.ReadAllBytes(fileDirectory);

            var uploadResponse =
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
            var firstSyncResponse = await _backendHttpClient.Get(
                $"{HomeServer}/_matrix/client/r0/sync?filter={_filterId}&access_token={AccessToken}");

            try
            {
                firstSyncResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Sync failed");
                return;
            }

            var firstSyncResponseContents = await firstSyncResponse.Content.ReadAsStringAsync();

            JObject firstSyncJObject = JObject.Parse(firstSyncResponseContents);
            var nextBatch = (string)firstSyncJObject["next_batch"];

            await Task.Delay(2000);

            while (!_syncCancellationToken.IsCancellationRequested)
            {
                var syncResponseMessage = await _backendHttpClient.Get(
                    $"{HomeServer}/_matrix/client/r0/sync?filter={_filterId}&since={nextBatch}&access_token={AccessToken}");

                try
                {
                    syncResponseMessage.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Sync failed");
                    return;
                }

                var syncResponseMessageContents = await syncResponseMessage.Content.ReadAsStringAsync();

                JObject syncResponseJObject = JObject.Parse(syncResponseMessageContents);

                nextBatch = (string)syncResponseJObject["next_batch"];

                await SyncRooms(nextBatch);

                await Task.Delay(2000, _syncCancellationToken.Token);
            }
        }

        private async Task SyncRooms(string nextBatch)
        {
            foreach (var room in _activeRoomsList)
            {
                var messageResponseMessage = await _backendHttpClient.Get(
                    $"{HomeServer}/_matrix/client/r0/rooms/{HttpUtility.UrlEncode(room.ChannelId)}/messages?from={nextBatch}&filter={_filterString}&dir=b&to={room.PrevBatch}&access_token={AccessToken}");

                var messageResponseMessageContent = await messageResponseMessage.Content.ReadAsStringAsync();

                var roomMessageParsed =
                    JsonConvert.DeserializeObject<RoomMessageJson>(messageResponseMessageContent);

                room.PrevBatch = roomMessageParsed.Start;

                if (roomMessageParsed.Chunk.Length == 0) continue;

                foreach (var message in roomMessageParsed.Chunk)
                {
                    if (message.Content == null) continue;

                    MessageReceivedEventArgs messageArg = new MessageReceivedEventArgs(message.RoomId, message.Content.Body, message.Sender);
                    MessageReceived?.Invoke(messageArg);
                }
            }
        }
    }
}
