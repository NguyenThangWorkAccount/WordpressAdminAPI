using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WordpressAdminApi.Controllers
{
    public partial class WordpressAdminController
    {
        private readonly ConcurrentQueue<ReportRequest> _notesQueue = new ConcurrentQueue<ReportRequest>();
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        #region Note API

        // POST /api/wordpressadministration/note
        [HttpPost("note")]
        public IActionResult WriteNote([FromBody] ReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LoginUrl) || request.IsSuccess == null)
            {
                return BadRequest(new { message = "LoginUrl and IsSuccess are required fields." });
            }

            _notesQueue.Enqueue(request);
            _ = Task.Run(() => ProcessNotesQueueAsync()); // Fire-and-forget to process asynchronously
            return CreatedAtAction(nameof(WriteNote), new { message = "Note added successfully." });
        }

        #endregion

        private async Task ProcessNotesQueueAsync()
        {
            await _queueSemaphore.WaitAsync();
            try
            {
                const string notesRange = "out"; // Assuming 'out' is the sheet name
                while (_notesQueue.TryDequeue(out var note))
                {
                    AppendNoteToSheet(note, notesRange);
                }
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }

        private void AppendNoteToSheet(ReportRequest note, string sheetRange)
        {
            var values = new List<object>
            {
                note.LoginUrl ?? string.Empty,
                $"[{note.Time}] {note.Message}",
                note.IsSuccess ?? false ? "Success" : "Failure"
            };

            var valueRange = new ValueRange { Values = new List<IList<object>> { values } };
            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _sheetId, sheetRange);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }
    }
}
