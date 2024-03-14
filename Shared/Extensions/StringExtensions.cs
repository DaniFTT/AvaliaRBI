using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account.Usage.Record;
using static iText.IO.Codec.TiffWriter;

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

    public static string NormalizeRG(this string value)
    {
        var regexUnformatted = new Regex(@"^(\d{2})(\d{3})(\d{3})(\d{1})$");
        var regexFormatted = new Regex(@"^\d{2}\.\d{3}\.\d{3}-\d{1}$");

        if (regexFormatted.IsMatch(value))
        {
            return value;
        }
        else if (regexUnformatted.IsMatch(value))
        {
            return regexUnformatted.Replace(value, @"$1.$2.$3-$4");
        }
        else
        {
            return string.Empty;
        }

    }
}

