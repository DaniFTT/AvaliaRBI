using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

