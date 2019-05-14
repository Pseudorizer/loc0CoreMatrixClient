namespace loc0NetMatrixClient
{
    /// <summary>
    /// Contains properties required for logging in to Matrix
    /// </summary>
    public class MatrixCredentials
    {
        public string UserName { get; set; }
        public string DeviceName { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }
}