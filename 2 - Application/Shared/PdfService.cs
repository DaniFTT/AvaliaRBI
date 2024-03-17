using AvaliaRBI._1___Presentation.Employees;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Models;
using AvaliaRBI.Shared.Extensions;
using BlazorTemplater;
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace AvaliaRBI._2___Application.Shared;

public class PdfService
{
    private readonly string styledHtmlTemplate;
    private EmailService EmailService;
    private NotificationsService NotificationsService;
    public static String FOOTER = "<table width=\"100%\" border=\"0\"><tr><td>Footer</td><td align=\"right\">Some title</td></tr></table>";


    public PdfService(EmailService emailService, NotificationsService notificationsService)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var bootstrapCssPath = System.IO.Path.Combine(basePath, "Resources", "Style", "bootstrap.min.css");
        var bootstrapOpenIconicCssPath = System.IO.Path.Combine(basePath, "Resources", "Style", "open-iconic-bootstrap.min.css");
        var appCssPath = System.IO.Path.Combine(basePath, "Resources", "Style", "app.css");
        var MudBlazorCssPath = System.IO.Path.Combine(basePath, "Resources", "Style", "MudBlazor.min.css");

        var bootstrapCssContent = File.ReadAllText(bootstrapCssPath);
        var bootstrapOpenIconicCssContent = File.ReadAllText(bootstrapOpenIconicCssPath);
        var appCssContent = File.ReadAllText(appCssPath);
        var mudBlazorContent = File.ReadAllText(MudBlazorCssPath);

        this.styledHtmlTemplate = $"<style>{bootstrapCssContent}{bootstrapOpenIconicCssContent}{appCssContent}{mudBlazorContent}</style>";

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

                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    CustomEndPageEventHandler handler = new CustomEndPageEventHandler(employee.Name, assessment.Responsible);
                    pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

                    PageSize pageSize = new PageSize(PageSize.A4);

                    pdfDocument.SetDefaultPageSize(pageSize);

                    HtmlConverter.ConvertToPdf(styledHtml, pdfDocument, new ConverterProperties());
                }

                fileBytes = memoryStream.ToArray();
            });

            return fileBytes;
        }
        finally
        {
            GC.Collect();
        }
    }

    public async IAsyncEnumerable<int> DownloadAllMonthlyAssessments(MonthlyAssessment assessment, AssessmentModel[] assessmentsModels)
    {
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Temp_{Guid.NewGuid()}");
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

        string zipPath = $"Relatorios_{assessment.ReferenceDate.Value:MM_yyyy}".GetFileName("zip");
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

        var pdfDestPath = baseFileName.GetFileName("pdf", path);

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

    private static string GetBaseMonthlyFileName(MonthlyAssessment assessment, Employee employee)
    {
        return $"AvaliacaoMensal-{employee.Name}-{assessment.ReferenceDate?.ToString("MM-yyyy")}";
    }
}

public class CustomEndPageEventHandler : IEventHandler
{
    private string employeeName;
    private string responsibleName;
    public CustomEndPageEventHandler(string employeeName, string responsibleName)
    {
        this.employeeName = employeeName;
        this.responsibleName = responsibleName;
    }

    //public void HandleEvent(Event @event)
    //{
    //    PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
    //    PdfDocument pdf = docEvent.GetDocument();
    //    PdfPage page = docEvent.GetPage();
    //    Rectangle pageSize = page.GetPageSize();
    //    PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

    //    float signatureAreaHeight = 80; // Aumentado para acomodar mais conteúdo
    //    float margin = 36; // Margem lateral para o conteúdo.
    //    float yPos = margin;

    //    Rectangle signatureArea = new Rectangle(margin, yPos, pageSize.GetWidth() - (2 * margin), signatureAreaHeight);

    //    Canvas canvas = new Canvas(pdfCanvas, signatureArea);

    //   canvas.Add(new Paragraph("Data da Assinatura: ")
    //       .SetFontSize(10).SetMarginTop(10)); // Ajuste o tamanho da fonte conforme necessário
    //    canvas.Add(new Paragraph("Nome completo do Responsável: ")
    //        .SetFontSize(10)); // Ajuste o tamanho da fonte conforme necessário
    //    canvas.Add(new Paragraph("Assinatura: ______________________")
    //        .SetFontSize(10)); // Ajuste o tamanho da fonte conforme necessário

    //    canvas.Close(); // Fechando o canvas para aplicar as alterações.
    //}

    //public void HandleEvent(Event @event)
    //{
    //    PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
    //    PdfDocument pdf = docEvent.GetDocument();
    //    PdfPage page = docEvent.GetPage();
    //    PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
    //    Rectangle pageSize = page.GetPageSize();

    //    // Definindo a área para cabeçalho e rodapé
    //    Rectangle footerArea = new Rectangle(36, pageSize.GetBottom(), pageSize.GetWidth(), 72);

    //    // Adicionando rodapé
    //    Canvas canvasFooter = new Canvas(pdfCanvas, footerArea);
    //    canvasFooter.Add(new Paragraph($"_____ de _________________ de {DateTime.Now.Year}").SetFontSize(10));

    //    canvasFooter.Add(new Paragraph("____________________________________").SetFontSize(10));
    //    canvasFooter.Add(new Paragraph(responsibleName).SetFontSize(10));

    //    canvasFooter.Close();
    //}

    public void HandleEvent(Event @event)
    {
        PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
        PdfDocument pdf = docEvent.GetDocument();
        PdfPage page = docEvent.GetPage();
        PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
        Rectangle pageSize = page.GetPageSize();


        Table table = new Table(2).UseAllAvailableWidth().SetStrokeWidth(0);

        iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell().SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT)
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetMarginTop(25)
            .SetMarginBottom(5)
            .Add(new Paragraph($"Avaliador Responsável").SetFontSize(10).SetPaddingBottom(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph("_____ de _________________ de ________").SetFontSize(10).SetPaddingBottom(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph("____________________________________").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph(responsibleName).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

        table.AddCell(cell);

        cell = new iText.Layout.Element.Cell().SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT)
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetMarginTop(25)
            .SetMarginBottom(5)
            .SetPaddingLeft(120)
            .Add(new Paragraph("Funcionário").SetFontSize(10).SetPaddingBottom(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph("_____ de _________________ de ________").SetFontSize(10).SetPaddingBottom(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph("____________________________________").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT))
            .Add(new Paragraph(employeeName).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

        table.AddCell(cell);

        table.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

        Rectangle signatureArea = new Rectangle(36, 15, page.GetPageSize().GetWidth() - 72, 90);

        Canvas canvasFooter = new Canvas(pdfCanvas, signatureArea);

        canvasFooter.Add(table);

        canvasFooter.Close();
    }
}