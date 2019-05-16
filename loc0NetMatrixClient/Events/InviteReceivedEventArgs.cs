using System;

namespace loc0NetMatrixClient.Events
{
    /// <summary>
    /// Args for InviteReceivedEvent
    /// </summary>
    public class InviteReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// RoomId invite came from
        /// </summary>
        public string RoomId { get; }

        /// <param name="roomId">RoomId invite came from</param>
        public InviteReceivedEventArgs(string roomId)
        {
            RoomId = roomId;
        }
    }
}