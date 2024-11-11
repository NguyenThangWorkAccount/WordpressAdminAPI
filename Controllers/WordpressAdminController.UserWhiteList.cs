using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using WordpressAdmin.API.Models;

namespace WordpressAdmin.API.Controllers
{
    public partial class WordpressAdminController
    {
        #region User White List API

        /// <summary>
        /// GET /api/wordpressadministration/user-white-list
        /// Retrieves the user white list from the Google Sheet.
        /// </summary>
        /// <returns>A list of users or a not found message if no data is available.</returns>
        [HttpGet("user-white-list")]
        public IActionResult GetUserWhiteList()
        {
            var userWhiteList = FetchUserWhiteList();
            if (userWhiteList.Any())
            {
                // Serialize with custom settings to remove null or empty string properties
                var json = JsonConvert.SerializeObject(userWhiteList, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new EmptyStringIgnoringContractResolver(), // Custom resolver
                    Formatting = Formatting.Indented
                });

                return Ok(json);
            }
            return NotFound(new { message = "No user white list data found." });
        }

        #endregion

        /// <summary>
        /// Fetches the user white list data from the Google Sheet.
        /// </summary>
        /// <returns>A collection of User objects.</returns>
        private IEnumerable<UserData> FetchUserWhiteList()
        {
            const string sheetRange = "white_list";
            var request = _sheetsService.Spreadsheets.Values.Get(_sheetId, sheetRange);
            var response = request.Execute();
            var rows = response.Values;

            if (rows == null || rows.Count <= 1)
            {
                return Enumerable.Empty<UserData>();
            }

            return rows.Skip(1).Select(row => new UserData
            {
                Username = row.ElementAtOrDefault(0)?.ToString(),
                Email = new Email
                {
                    Name = row.ElementAtOrDefault(1)?.ToString(),
                    DomainStatus = row.ElementAtOrDefault(2)?.ToString(),
                    DomainValue = row.ElementAtOrDefault(3)?.ToString()
                },
                Role = row.ElementAtOrDefault(4)?.ToString(),
                Password = new Password
                {
                    Mode = row.ElementAtOrDefault(5)?.ToString(),
                    Value = row.ElementAtOrDefault(6)?.ToString()
                }
            })
            .Where(user => !string.IsNullOrWhiteSpace(user.Username) && !string.IsNullOrWhiteSpace(user.Email?.Name))
            .Select(user =>
            {
                if (user.Email != null)
                {
                    if (user.Email.DomainStatus == "manual" && string.IsNullOrWhiteSpace(user.Email.DomainValue))
                    {
                        user.Email.DomainValue = null;
                    }
                    else if (user.Email.DomainStatus == "auto" && user.Email.DomainValue == null)
                    {
                        user.Email.DomainValue = "";
                    }
                }

                if (user.Password != null)
                {
                    if (user.Password.Mode == "manual" && string.IsNullOrWhiteSpace(user.Password.Value))
                    {
                        user.Password.Value = null;
                    }
                    else if (user.Password.Mode == "auto" && user.Password.Value == null)
                    {
                        user.Password.Value = "";
                    }
                }

                return user;
            });
        }
    }

    #region Custom Contract Resolver

    public class EmptyStringIgnoringContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            property.ShouldSerialize = instance =>
            {
                var value = property?.ValueProvider?.GetValue(instance);
                return value switch
                {
                    null => false,
                    string str => !string.IsNullOrWhiteSpace(str),
                    _ => true
                };
            };

            return property;
        }
    }

    #endregion
}
