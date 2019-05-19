using Newtonsoft.Json;

namespace loc0CoreMatrixClient.Models
{
    /// <summary>
    /// Defines an object for storing a messages information
    /// </summary>
    public class MatrixTextMessage
    {
        /// <summary>
        /// HTML formatted body
        /// </summary>
        [JsonProperty("formatted_body")]
        public string FormattedBody { get; set; } = "";

        /// <summary>
        /// Format of the message, unless formatted body is being used it should remain empty
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; } = "";

        /// <summary>
        /// Plain text body
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; } = "";

        /// <summary>
        /// Message type is always m.text
        /// </summary>
        [JsonProperty("msgtype")]
        public string MsgType { get; } = "m.text";
    }
}