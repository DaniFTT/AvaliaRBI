using AvaliaRBI._1___Presentation.Employees;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Models;
using BlazorTemplater;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace AvaliaRBI._2___Application.Shared;

public class PdfService
{
    private readonly string styledHtmlTemplate;
    private readonly string downloadPath;
    private EmailService EmailService;
    private NotificationsService NotificationsService;

    public PdfService(EmailService emailService, NotificationsService notificationsService)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var bootstrapCssPath = Path.Combine(basePath, "Resources", "Style", "bootstrap.min.css");
        var bootstrapOpenIconicCssPath = Path.Combine(basePath, "Resources", "Style", "open-iconic-bootstrap.min.css");
        var appCssPath = Path.Combine(basePath, "Resources", "Style", "app.css");
        var MudBlazorCssPath = Path.Combine(basePath, "Resources", "Style", "MudBlazor.min.css");

        var bootstrapCssContent = File.ReadAllText(bootstrapCssPath);
        var bootstrapOpenIconicCssContent = File.ReadAllText(bootstrapOpenIconicCssPath);
        var appCssContent = File.ReadAllText(appCssPath);
        var mudBlazorContent = File.ReadAllText(MudBlazorCssPath);

        this.styledHtmlTemplate = $"<style>{bootstrapCssContent}{bootstrapOpenIconicCssContent}{appCssContent}{mudBlazorContent}</style>";

        string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        this.downloadPath = Path.Combine(pathUser, "Downloads\\");

        EmailService = emailService;
        NotificationsService = notificationsService;
    }

    public async Task<byte[]> GeneratePdf(string html, MonthlyAssessment assessment, Employee employee)
    {
        byte[] fileBytes = Array.Empty<byte>();
        try
        {
            using var memoryStream = new System.IO.MemoryStream();
            using var pdfWriter = new PdfWriter(memoryStream);
            string baseFileName = GetBaseMonthlyFileName(assessment, employee);

            await Task.Run(() =>
            {
                var styledHtml = this.styledHtmlTemplate + html;

                HtmlConverter.ConvertToPdf(styledHtml, pdfWriter);

                fileBytes = memoryStream.ToArray();
            });

            return fileBytes;
        }
        finally
        {
            GC.Collect();
        }
    }

    public async Task SendMonthlyReportsByWpp(MonthlyAssessment assessment, AssessmentModel[] assessmentsModels)
    {
        try
        {
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 4
            };

            var employees = assessmentsModels.SelectMany(a => a.Employees).ToArray();
            var assessmentsEmployees = assessmentsModels.SelectMany(a => a.AssessmentEmployees).ToArray();

            await Parallel.ForEachAsync(employees, options, async (employee, ct) =>
            {

                var assessmentEmployee = assessmentsEmployees.FirstOrDefault(ae => ae.EmployeeId == employee.Id);
                if (assessmentEmployee != null)
                {
                    await DownloadMonthlyEmployeeReport(assessment, employee, assessmentEmployee);
                }
            });

            NotificationsService.AddNotification($"Relatórios da Avaliação Mensal {assessment.ReferenceDate.Value.ToString("MM/yyyy")} enviados com sucesso!");
        }
        catch (Exception)
        {
            NotificationsService.AddNotification($"Erro ao enviar os Relatórios da Avaliação Mensal {assessment.ReferenceDate.Value.ToString("MM/yyyy")}!", Notification.NotificationType.Error);
        }
        finally
        {
            GC.Collect();
        }
    }

    public delegate void ProgressReport(int completedDownloads, int totalDownloads);

    public async IAsyncEnumerable<int> DownloadAllMonthlyAssessments(MonthlyAssessment assessment, AssessmentModel[] assessmentsModels)
    {
        string tempPath = string.Empty;

        tempPath = Path.Combine(Path.GetTempPath(), $"Temp_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);

        var employees = assessmentsModels.SelectMany(a => a.Employees).ToArray();
        var assessmentsEmployees = assessmentsModels.SelectMany(a => a.AssessmentEmployees).ToArray();

        int completedDownloads = 0;

        foreach (var employee in employees)
        {
            var assessmentEmployee = assessmentsEmployees.FirstOrDefault(ae => ae.EmployeeId == employee.Id);
            if (assessmentEmployee != null)
            {
                await DownloadMonthlyEmployeeReport(assessment, employee, assessmentEmployee, tempPath);
                yield return ++completedDownloads;
            }
        }

        string zipPath = GetFileName($"Relatorios_{assessment.ReferenceDate.Value:MM_yyyy}", "zip");
        ZipFile.CreateFromDirectory(tempPath, zipPath);

        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);

        GC.Collect(1);
    }

    public async Task DownloadMonthlyEmployeeReport(MonthlyAssessment assessment, Employee employee, AssessmentEmployee assessmentEmployee, string path = null)
    {
        string html = GetHtmlEmployeeReport(assessment, employee, assessmentEmployee);
        string baseFileName = GetBaseMonthlyFileName(assessment, employee);

        var fileBytes = await GeneratePdf(html, assessment, employee);

        var pdfDestPath = GetFileName(baseFileName, "pdf", path);

        File.WriteAllBytes(pdfDestPath, fileBytes);
    }

    private string GetHtmlEmployeeReport(MonthlyAssessment assessment, Employee employee, AssessmentEmployee assessmentEmployee)
    {
        var aspects = assessmentEmployee.AssessmentCollections.SelectMany(ac => ac.AssessmentAspects).ToList();
        var criteria = aspects.SelectMany(aa => aa.Criteria).ToList();

        var totalPerfomance = assessmentEmployee.AssessmentCollections.Sum(a => a.PercentageSum) / (assessmentEmployee.AssessmentCollections.Sum(a => a.QtdCriterias));

        string html = new ComponentRenderer<AssessmentEmployeeReport>()
                        .AddService(this)
                        .AddService(EmailService)
                        .AddService(NotificationsService)
                        .AddService(new ExcelService(EmailService, NotificationsService))
                        .AddService<ILoggerFactory>(LoggerFactory.Create(builder =>
                        {
                            builder.AddDebug();
                        }))
                        .Set(c => c.assessment, assessment)
                        .Set(c => c.employee, employee)
                        .Set(c => c.assessmentEmployee, assessmentEmployee)
                        .Set(c => c.aspects, aspects)
                        .Set(c => c.criteria, criteria)
                        .Set(c => c.totalPerfomance, totalPerfomance)
                        .Render();
        return html;
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

    private static string GetBaseMonthlyFileName(MonthlyAssessment assessment, Employee employee)
    {
        return $"AvaliacaoMensal-{employee.Name}-{assessment.ReferenceDate?.ToString("MM-yyyy")}";
    }
}

