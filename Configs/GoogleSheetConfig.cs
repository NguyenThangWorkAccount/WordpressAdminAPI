namespace WordpressAdminApi
{
    public class GoogleSheetsConfig
    {
        public string ApiKeyPath { get; }
        public string SheetId { get; }

        public GoogleSheetsConfig(string apiKeyPath, string sheetId)
        {
            if (string.IsNullOrWhiteSpace(apiKeyPath))
                throw new ArgumentException("ApiKeyPath cannot be null or empty.", nameof(apiKeyPath));

            if (string.IsNullOrWhiteSpace(sheetId))
                throw new ArgumentException("SheetId cannot be null or empty.", nameof(sheetId));

            ApiKeyPath = apiKeyPath;
            SheetId = sheetId;
        }
    }
}
