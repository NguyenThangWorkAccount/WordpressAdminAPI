using Microsoft.AspNetCore.Mvc;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace WordpressAdminApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class WordpressAdminController : ControllerBase
    {
        private readonly string _sheetId;
        private readonly SheetsService _sheetsService;

        // Constructor to initialize Google Sheets API and sheet ID
        public WordpressAdminController(IConfiguration configuration, SheetsService sheetsService)
        {
            _sheetId = configuration["GoogleSheetsConfig:SheetId"];
            _sheetsService = sheetsService;
        }
    }
}