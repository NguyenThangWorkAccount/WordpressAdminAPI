namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        public class ReportRequest
        {
            public string LoginUrl { get; set; } = string.Empty;
            public bool? IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime Time { get; set; } = DateTime.UtcNow;
        }
    }
}
