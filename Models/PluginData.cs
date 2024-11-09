namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        public class PluginData
        {
            public string Title { get; }
            public string InstallPath { get; }
            public bool NeedToSetup { get; }
            public Dictionary<string, string> AdditionalSetupFields { get; }

            public PluginData(string title, string installPath, bool needToSetup, Dictionary<string, string> additionalSetupFields)
            {
                if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));

                Title = title;
                InstallPath = installPath;
                NeedToSetup = needToSetup;
                AdditionalSetupFields = additionalSetupFields ?? new Dictionary<string, string>();
            }

            // Serializes only relevant fields based on conditions
            public Dictionary<string, object> ToSerializableDictionary()
            {
                var result = new Dictionary<string, object>
                {
                    { "title", Title }
                };

                // Add InstallPath only if it's not empty
                if (!string.IsNullOrEmpty(InstallPath))
                {
                    result.Add("installPath", InstallPath);
                }

                // Add NeedToSetup only if it's true
                if (NeedToSetup)
                {
                    result.Add("needToSetup", NeedToSetup);

                    // Add all fields in AdditionalSetupFields to the top level
                    if (AdditionalSetupFields != null)
                    {
                        foreach (var item in AdditionalSetupFields)
                        {
                            Console.WriteLine(item.ToString());
                        }
                        foreach (var field in AdditionalSetupFields)
                        {
                            result[field.Key] = field.Value; // Add each field directly to the main dictionary
                        }
                    }
                }

                return result;
            }


        }
    }
}
