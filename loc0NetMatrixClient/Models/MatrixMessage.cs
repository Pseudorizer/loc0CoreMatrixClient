namespace loc0NetMatrixClient.Models
{
    /// <summary>
    /// Defines an object for storing a messages information
    /// </summary>
    public class MatrixMessage
    {

        /// <summary>
        /// Message type I.E. m.text
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Format of the information I.E. org.matrix.custom.html
        /// </summary>
        public string Format { get; set; }

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