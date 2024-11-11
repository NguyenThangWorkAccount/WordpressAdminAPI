using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.IO;

namespace WordpressAdmin.API.Utilities
{
    public class SheetsServiceInitializer
    {
        private readonly string apiKeyPath;

        public SheetsServiceInitializer(string apiKeyPath)
        {
            if (string.IsNullOrEmpty(apiKeyPath))
            {
                throw new ArgumentNullException(nameof(apiKeyPath), "The API key path cannot be null or empty.");
            }
            this.apiKeyPath = apiKeyPath;
        }

        public SheetsService InitializeSheetsService()
        {
            if (!File.Exists(apiKeyPath))
            {
                throw new FileNotFoundException("The API key file was not found at the specified path.", apiKeyPath);
            }

            GoogleCredential credential;
            using (var stream = new FileStream(apiKeyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WordpressAdmin.API"
            });
        }
    }
}
