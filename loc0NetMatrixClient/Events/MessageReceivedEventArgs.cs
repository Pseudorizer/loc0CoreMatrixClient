using System;

namespace loc0NetMatrixClient.Events
{
    /// <summary>
    /// Args for MessageReceivedEvent
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public string RoomId { get; }
        public string Message { get; }
        public string SenderId { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomId">RoomID from message chunk</param>
        /// <param name="message">Body from message chunk</param>
        /// <param name="senderId">SenderID from message chunk</param>
        public MessageReceivedEventArgs(string roomId, string message, string senderId)
        {
            RoomId = roomId;
            Message = message;
            SenderId = senderId;
        }
    }
}