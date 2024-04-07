using AvaliaRBI._3___Domain;
using System.Globalization;
using System.Text.RegularExpressions;

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

    public static string NormalizeCPF(this string value)
    {
        var regexUnformatted = new Regex(@"^(\d{3})(\d{3})(\d{3})(\d{2})$");
        var regexFormatted = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");

        if (regexFormatted.IsMatch(value))
            return value;
        else if (regexUnformatted.IsMatch(value))
            return regexUnformatted.Replace(value, @"$1.$2.$3-$4");
        else
            return string.Empty;

    }

    public static string GetFormatedDate(this DateTime date)
    {
        CultureInfo cultura = new CultureInfo("pt-BR");

        string mes = cultura.TextInfo.ToTitleCase(cultura.DateTimeFormat.GetMonthName(date.Month));

        string ano = date.ToString("yyyy");

        return $"{mes}/{ano}";
    }

    public static string GetScratchMessage(this MonthlyAssessment assessment) => $"Rascunho de Avaliação ({assessment.ReferenceDate.Value.ToString("MM/yyyy")})...";
    public static string GetConfirmationMessage(this MonthlyAssessment assessment) => $"Avaliação Finalizada {(assessment.ClosedDate.HasValue ? "(" + assessment.ClosedDate.Value.ToString("dd/MM/yyyy") + ")" : string.Empty)}";
}

