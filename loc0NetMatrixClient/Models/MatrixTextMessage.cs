namespace loc0NetMatrixClient.Models
{
    /// <summary>
    /// Defines an object for storing a messages information
    /// </summary>
    public class MatrixTextMessage
    {
        /// <summary>
        /// HTML formatted body
        /// </summary>
        public string FormattedBody { get; set; }

        /// <summary>
        /// Plain text body
        /// </summary>
        public string Body { get; set; }
    }
}