using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account.Usage.Record;

namespace AvaliaRBI.Shared.Extensions;

public static class StringExtensions
{
    private static string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads\\");

    public static string NormalizeString(this string text)
    {
        text = text.Trim();
        return Regex.Replace(text, @"\s{2,}", " ");
    }

    public static string GetFileName(this string baseFileName, string extension, string directoryPath = null)
    {
        string fullPath;
        int fileCount = 1;
        do
        {
            string fileName = $"{baseFileName}{(fileCount > 1 ? $"-{fileCount}" : string.Empty)}.{extension}";
            fullPath = Path.Combine(directoryPath ?? downloadPath, fileName);
            fileCount++;
        } while (File.Exists(fullPath));

        return fullPath;
    }
}

