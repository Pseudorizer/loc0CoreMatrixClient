using System;

namespace loc0CoreMatrixClient.Events
{
    /// <summary>
    /// Args for MessageReceivedEvent
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {

        /// <summary>
        /// Origin RoomID of the message
        /// </summary>
        public string RoomId { get; }

        /// <summary>
        /// Plain text message body content
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// ID of the user sending the message
        /// </summary>
        public string SenderId { get; }

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