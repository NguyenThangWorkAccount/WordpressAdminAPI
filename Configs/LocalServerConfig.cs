namespace WordpressAdminApi
{
    public class LocalServerConfig
    {
        public string Port { get; }

        public LocalServerConfig(string port)
        {
            if (string.IsNullOrWhiteSpace(port))
                throw new ArgumentException("Port cannot be null or empty.", nameof(port));

            Port = port;
        }
    }
}
