
/* Unmerged change from project 'AvaliaRBI (net6.0-android)'
Before:
using MimeKit.Text;
using AvaliaRBI._3___Domain.Models;
After:
using AvaliaRBI._3___Domain.Models;
using MimeKit.Text;
*/

/* Unmerged change from project 'AvaliaRBI (net6.0-maccatalyst)'
Before:
using MimeKit.Text;
using AvaliaRBI._3___Domain.Models;
After:
using AvaliaRBI._3___Domain.Models;
using MimeKit.Text;
*/

/* Unmerged change from project 'AvaliaRBI (net6.0-ios)'
Before:
using MimeKit.Text;
using AvaliaRBI._3___Domain.Models;
After:
using AvaliaRBI._3___Domain.Models;
using MimeKit.Text;
*/
using MimeKit.Text;

namespace AvaliaRBI._3___Domain.Models;

public class EmailModel
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public TextFormat Format { get; set; }
    public FileInfo File { get; set; }
}

