namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        private IList<IList<object>> GetSheetData(string sheetName)
        {

            var request = _sheetsService.Spreadsheets.Values.Get(_sheetId, sheetName);
            var response = request.Execute();
            return response?.Values ?? new List<IList<object>>();
        }
    }
}