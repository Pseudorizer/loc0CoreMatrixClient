using Newtonsoft.Json;

namespace loc0CoreMatrixClient.Models
{
    /// <summary>
    /// Contains properties required for logging in to Matrix
    /// </summary>
    public class MatrixCredentials
    {
        /// <summary>
        /// Username of the account
        /// </summary>
        [JsonProperty("user")]
        public readonly string UserName;

        /// <summary>
        /// Desired device name, if it's not specified one will be auto-generated on every login
        /// </summary>
        [JsonProperty("initial_device_display_name")]
        public readonly string DeviceName;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type = "m.login.password";

        /// <summary>
        /// Account password
        /// </summary>
        [JsonProperty("password")]
        public readonly string Password;

        /// <summary>
        /// Desired device id, if it's not specified one will be auto-generated on every login
        /// </summary>
        [JsonProperty("device_id")]
        public readonly string DeviceId;

        /// <summary>
        /// Credentials required for logging in
        /// </summary>
        /// <param name="userName">Username for account</param>
        /// <param name="password">Password for account</param>
        /// <param name="deviceId">Desired device ID, if left empty one will be auto generated</param>
        /// <param name="deviceName">Desired device name, if left empty one will be auto generated</param>
        public MatrixCredentials(string userName, string password, string deviceId = null, string deviceName = null)
        {
            DeviceId = deviceId;
            Password = password;
            DeviceName = deviceName;
            UserName = userName;
        }
    }
}
