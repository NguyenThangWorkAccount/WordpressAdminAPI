using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        #region Plugin White List API

        // GET /api/wordpressadministration/plugin-white-list
        [HttpGet("plugin-white-list")]
        public IActionResult GetPluginWhiteList()
        {
            var pluginWhiteList = FetchPluginWhitelistData();
            var serializedPluginList = pluginWhiteList.Select(p => p.ToSerializableDictionary()).ToList();
            return serializedPluginList.Any() ? Ok(serializedPluginList) : NotFound(new { message = "No plugin whitelist data found." });
        }

        #endregion

        private IEnumerable<PluginData> FetchPluginWhitelistData()
        {
            var pluginRows = GetSheetData("plugin_white_list");
            return pluginRows == null || pluginRows.Count <= 1 ? Enumerable.Empty<PluginData>() : ParsePluginWhitelistRows(pluginRows);
        }

        private IEnumerable<PluginData> ParsePluginWhitelistRows(IList<IList<object>> rows)
        {
            // Skip header row and parse data rows
            return rows.Skip(2)
                       .Where(row => !string.IsNullOrWhiteSpace(row.ElementAtOrDefault(0)?.ToString())) // Ensure Title is not empty
                       .Select(row =>
                       {
                           var title = row.ElementAtOrDefault(0)?.ToString() ?? string.Empty;
                           var installPath = row.ElementAtOrDefault(1)?.ToString();
                           var needToSetup = bool.TryParse(row.ElementAtOrDefault(2)?.ToString(), out var setup) && setup;
                           var additionalSetupInfoJson = row.ElementAtOrDefault(3)?.ToString();
                           var additionalSetupInfo = ParseAdditionalSetupInfo(additionalSetupInfoJson ?? string.Empty);

                           // Create the PluginData instance
                           return new PluginData(title, installPath ?? string.Empty, needToSetup, additionalSetupInfo);
                       });
        }

        private Dictionary<string, string> ParseAdditionalSetupInfo(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>();

            // Replace single backslashes with double backslashes
            json = json.Replace(@"\", @"\\");

            Console.WriteLine("Parsing JSON with escaped paths: " + json); // Debug output

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message); // Log error if parsing fails
                return new Dictionary<string, string>(); // Return empty if JSON is invalid
            }
        }
    }
}
