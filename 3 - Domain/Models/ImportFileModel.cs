using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AvaliaRBI._3___Domain.Models;

public class ImportFileModel
{
    public string ProcessId { get; set; }
    public IBrowserFile File { get; set; }
    public string Extension { get; set; }
    public EventCallback<ImportFileModel> UploadCallBack { get; set; }

    public Stream GetFileBytes()
    {
        return File.OpenReadStream();
    }
}

