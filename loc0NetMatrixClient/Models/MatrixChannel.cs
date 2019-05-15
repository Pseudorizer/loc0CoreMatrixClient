namespace loc0NetMatrixClient.Models
{
    internal class MatrixChannel
    {
        public string PrevBatch { get; set; }
        public string ChannelId { get; }

        public MatrixChannel(string channelId)
        {
            ChannelId = channelId;
        }
    }
}