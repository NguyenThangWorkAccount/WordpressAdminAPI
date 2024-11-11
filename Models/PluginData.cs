using System;
using System.Collections.Generic;

namespace WordpressAdmin.API.Controllers
{
    public partial class WordpressAdminController
    {
        public class PluginData
        {
            public string Title { get; }
            public string Slug { get; }
            public string InstallPath { get; }
            public bool NeedToSetup { get; }
            public Dictionary<string, string> AdditionalSetupFields { get; }

            public PluginData(string title, string slug, string installPath, bool needToSetup, Dictionary<string, string> additionalSetupFields)
            {
                // Ensure that at least one of Title or Slug is provided
                if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(slug))
                    throw new ArgumentException("Either 'title' or 'slug' must be provided.");

                Title = title;
                Slug = slug;
                InstallPath = installPath;
                NeedToSetup = needToSetup;
                AdditionalSetupFields = additionalSetupFields ?? new Dictionary<string, string>();
            }

            // Serializes only relevant fields based on conditions
            public Dictionary<string, object> ToSerializableDictionary()
            {
                var result = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(Title))
                {
                    result.Add("Title", Title);
                }

                if (!string.IsNullOrEmpty(Slug))
                {
                    result.Add("Slug", Slug);
                }

                if (!string.IsNullOrEmpty(InstallPath))
                {
                    result.Add("InstallPath", InstallPath);
                }

                if (NeedToSetup)
                {
                    result.Add("NeedToSetup", NeedToSetup);

                    foreach (var field in AdditionalSetupFields)
                    {
                        result[field.Key] = field.Value;
                    }
                }

                return result;
            }
        }
    }
}
