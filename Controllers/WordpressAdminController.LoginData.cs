using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using WordpressAdminApi.Models;

namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        private static readonly HashSet<int> _processedRowIndices = new HashSet<int>();
        private static readonly object _lock = new object(); // Lock for thread safety

        // GET /api/wordpressadministration/login-data
        [HttpGet("login-data")]
        public IActionResult GetNextAvailableLoginData()
        {
            var loginData = FetchNextLoginDataFromSheet();
            if (loginData != null)
            {
                return Ok(loginData);
            }
            return NotFound(new { message = "No more login data available." });
        }

        private LoginData? FetchNextLoginDataFromSheet()
        {
            const string sheetName = "in"; // Assumes 'in' is the sheet name
            var request = _sheetsService.Spreadsheets.Values.Get(_sheetId, sheetName);
            var response = request.Execute();
            var rows = response.Values;

            if (rows == null || rows.Count <= 1)
            {
                return null; // No data available beyond the header
            }

            lock (_lock)
            {
                // Iterate over rows to find the next unprocessed row
                for (int i = 1; i < rows.Count; i++)
                {
                    if (_processedRowIndices.Contains(i))
                    {
                        continue; // Skip already processed rows
                    }

                    var row = rows[i];
                    var loginUrls = row.ElementAtOrDefault(0)?.ToString()?.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var usernames = row.ElementAtOrDefault(1)?.ToString()?.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var passwords = row.ElementAtOrDefault(2)?.ToString()?.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Skip if any of the required fields is empty
                    if (loginUrls == null || !loginUrls.Any() || usernames == null || !usernames.Any() || passwords == null || !passwords.Any())
                    {
                        continue;
                    }

                    _processedRowIndices.Add(i); // Mark this row as processed

                    return new LoginData(loginUrls, usernames, passwords);
                }
            }

            return null; // No more available data
        }
    }
}
