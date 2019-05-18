namespace loc0CoreMatrixClient.Models
{
    /// <summary>
    /// Defines an object for storing information about a file for sending
    /// </summary>
    public class MatrixFileMessage
    {
        /// <summary>
        /// Filename to be used
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Message type I.E. m.image
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// MxcUrl to be used when sending message
        /// </summary>
        public string MxcUrl { get; set; }
    }
}