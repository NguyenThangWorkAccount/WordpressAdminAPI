using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace WordpressAdminAPI.Utilities
{
    public class SheetsServiceInitializer
    {
        private readonly string apiKeyPath;

        public SheetsServiceInitializer(string apiKeyPath)
        {
            this.apiKeyPath = apiKeyPath;
        }

        public SheetsService InitializeSheetsService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(apiKeyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WordpressAdminApi"
            });
        }
    }
}
