namespace loc0NetMatrixClient.Models
{
    internal class Channel
    {
        public string PrevBatch { get; set; }
        public string ChannelId { get; }

        public Channel(string channelId)
        {
            ChannelId = channelId;
        }

    }
}