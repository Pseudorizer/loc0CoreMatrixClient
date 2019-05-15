using Newtonsoft.Json;

namespace loc0NetMatrixClient.Models
{
    internal class RoomMessageJson
    {
        [JsonProperty("chunk")]
        public Chunk[] Chunk { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }

    internal class Chunk
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("room_id")]
        public string RoomId { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("origin_server_ts")]
        public long OriginServerTs { get; set; }

        [JsonProperty("unsigned")]
        public Unsigned Unsigned { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("age")]
        public long Age { get; set; }

        [JsonProperty("state_key", NullValueHandling = NullValueHandling.Ignore)]
        public string StateKey { get; set; }
    }

    internal class Content
    {
        [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; }

        [JsonProperty("formatted_body")]
        public object FormattedBody { get; set; }

        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; }

        [JsonProperty("msgtype", NullValueHandling = NullValueHandling.Ignore)]
        public string Msgtype { get; set; }

        [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayname", NullValueHandling = NullValueHandling.Ignore)]
        public string Displayname { get; set; }

        [JsonProperty("membership", NullValueHandling = NullValueHandling.Ignore)]
        public string Membership { get; set; }
    }

    internal class Unsigned
    {
        [JsonProperty("age")]
        public long Age { get; set; }
    }
}
