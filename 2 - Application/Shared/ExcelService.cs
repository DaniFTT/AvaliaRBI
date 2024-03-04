using AvaliaRBI._3___Domain.Models;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaliaRBI._2___Application.Shared;

public class ExcelService
{
    private readonly string downloadPath;
    private EmailService EmailService;
    private NotificationsService NotificationsService;

    public ExcelService(EmailService emailService, NotificationsService notificationsService)
    {
        string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        this.downloadPath = Path.Combine(pathUser, "Downloads\\");

        EmailService = emailService;
        NotificationsService = notificationsService;
    }

    public async Task SalvarExcel(string baseFileName, byte[] fileBytes, EmailModel email = null)
    {
        try
        {
            var pdfDestPath = GetFileName(baseFileName, "xlsx");

            File.WriteAllBytes(pdfDestPath, fileBytes);

            if (email != null)
            {
                email.File = new _3___Domain.Models.FileInfo
                {
                    FileBytes = fileBytes,
                    FileName = baseFileName,
                    Extension = "xlsx"
                };

                await EmailService.SendEmailAsync(email);
            }

        }
        catch (Exception e)
        {
        }
    }

    private string GetFileName(string baseFileName, string extension, string directoryPath = null)
    {
        string fullPath;
        int fileCount = 1;
        do
        {
            string fileName = $"{baseFileName}{(fileCount > 1 ? $"-{fileCount}" : string.Empty)}.{extension}";
            fullPath = Path.Combine(directoryPath ?? this.downloadPath, fileName);
            fileCount++;
        } while (File.Exists(fullPath));

        return fullPath;
    }
}

