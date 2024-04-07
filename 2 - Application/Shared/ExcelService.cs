using AvaliaRBI._3___Domain.Models;
using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._2___Application.Shared;

public class ExcelService
{
    private EmailService EmailService;
    private NotificationsService NotificationsService;

    public ExcelService(EmailService emailService, NotificationsService notificationsService)
    {
        EmailService = emailService;
        NotificationsService = notificationsService;
    }

    public async Task SalvarExcel(string baseFileName, byte[] fileBytes, EmailModel email = null)
    {
        try
        {
            var pdfDestPath = baseFileName.GetFileName("xlsx");

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
}

