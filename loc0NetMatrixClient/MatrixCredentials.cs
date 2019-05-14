namespace loc0NetMatrixClient
{
    /// <summary>
    /// Contains properties required for logging in to Matrix
    /// </summary>
    public class MatrixCredentials
    {
        /// <summary>
        /// Username of the account
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Desired device name, if it's not specified one will be auto-generated on every login
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Account password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Desired device name, if it's not specified one will be auto-generated on every login
        /// </summary>
        public string DeviceId { get; set; }
    }
}