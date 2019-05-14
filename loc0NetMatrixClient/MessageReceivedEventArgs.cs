using System;

namespace loc0NetMatrixClient
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string RoomId { get; }
        public string Message { get; }
        public string SenderId { get; }

        public MessageReceivedEventArgs(string roomId, string message, string senderId)
        {
            RoomId = roomId;
            Message = message;
            SenderId = senderId;
        }
    }
}