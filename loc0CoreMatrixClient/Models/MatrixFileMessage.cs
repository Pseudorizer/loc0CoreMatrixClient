using Newtonsoft.Json;

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
        [JsonProperty("filename")]
        public string Filename { get; set; } = "";

        /// <summary>
        /// A description for the file, defaults to the filename
        /// </summary>
        [JsonProperty("body")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Empty info class for now
        /// </summary>
        [JsonProperty("info")]
        public Info InfoObj { get; } = new Info();

        /// <summary>
        /// 
        /// </summary>
        public class Info { }

        /// <summary>
        /// Message type I.E. m.image
        /// </summary>
        [JsonProperty("msgtype")]
        public string Type { get; set; } = "";

        /// <summary>
        /// MxcUrl to be used when sending message
        /// </summary>
        [JsonProperty("url")]
        public string MxcUrl { get; set; } = "";
    }
}