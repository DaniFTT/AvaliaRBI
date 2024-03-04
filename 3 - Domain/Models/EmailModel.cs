using MimeKit.Text;
using AvaliaRBI._3___Domain.Models;

namespace AvaliaRBI._3___Domain.Models;

public class EmailModel
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public TextFormat Format { get; set; }
    public FileInfo File { get; set; }
}

