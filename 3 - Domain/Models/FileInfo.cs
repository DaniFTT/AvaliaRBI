using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaliaRBI._3___Domain.Models;

public class FileInfo
{
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string Extension { get; set; }
}

