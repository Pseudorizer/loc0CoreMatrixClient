using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using loc0CoreMatrixClient.Events;
using Newtonsoft.Json.Linq;

namespace loc0CoreMatrixClient
{
    internal class MatrixListener
    {
        private MatrixHttp _backendHttpClient;
        private MatrixClient _client;

        /// <summary>
        /// Contact the sync endpoint in a fire and forget background task
        /// </summary>
        internal async Task Sync(MatrixClient client, CancellationToken syncCancellationToken)
        {
            _client = client;
            _backendHttpClient = new MatrixHttp(client.HomeServer, client.AccessToken);

            var nextBatch = string.Empty;

            while (string.IsNullOrWhiteSpace(nextBatch) && !syncCancellationToken.IsCancellationRequested)
            {
                nextBatch = await FirstSync();
                await Task.Delay(2000, syncCancellationToken);
            }

            while (!syncCancellationToken.IsCancellationRequested)
            {
                HttpResponseMessage syncResponseMessage = await _backendHttpClient.Get(
                    $"/_matrix/client/r0/sync?filter={_client.FilterId}&since={nextBatch}", true);
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
                $"/_matrix/client/r0/sync?filter={_client.FilterId}", true);

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

                    if (!_client.Rooms.ContainsKey(roomId)) continue;

                    foreach (JToken message in room.First["timeline"]["events"].Children())
                    {
                        if (!message["content"].HasValues) continue;

                        var sender = (string)message["sender"];
                        var body = (string)message["content"]["body"];

                        var messageArgs = new MessageReceivedEventArgs(roomId, body, sender);
                        _client.OnMessageReceived(messageArgs);
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
                    _client.OnInviteReceived(inviteArgs);
                }
            }
        }
    }
}