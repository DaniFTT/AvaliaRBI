using Ardalis.Result;
using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._3___Domain.Models;
using AvaliaRBI._4___Repository;
using AvaliaRBI.Shared.Extensions;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;

namespace AvaliaRBI._2___Application;

public class MonthlyAssessmentService : BaseService<MonthlyAssessment>
{
    private MonthlyAssessmentRepository _repository;
    private ExcelService _excelService;
    private EmailService _emailService;
    private static System.Drawing.Color darkBlue = System.Drawing.Color.FromArgb(14, 40, 65);

    public MonthlyAssessmentService(IBaseRepository<MonthlyAssessment> repository,
        ExcelService excelService, EmailService emailService, NotificationsService notificationsService) : base(repository, notificationsService)
    {
        _repository = repository as MonthlyAssessmentRepository;
        _excelService = excelService;
        _emailService = emailService;
    }

    public async Task<IEnumerable<MonthlyAssessment>> GetByReferenceDate(DateTime referenceDate)
    {
        return await _repository.GetByReferenceDate(referenceDate);
    }

    public async Task<Result<MonthlyAssessment>> GetByIdUpdated(int id)
    {
        try
        {
            return Result<MonthlyAssessment>.Success(await _repository.GetByIdUpdated(id));
        }
        catch (Exception e)
        {
            return Result<MonthlyAssessment>.Error("Erro obter a Avaliação Mensal");
        }
    }

    public async Task<bool> ExportMonthlyAssessmentToExcel(string processId, MonthlyAssessment assessment, List<AssessmentModel> assessmentModels)
    {
        Notification notification = null;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        try
        {
            notification = new Notification($"O Relatório de Avaliação Mensal está sendo gerado", Convert.ToDouble(assessmentModels.Count), processId);
            _notificationsService.AddNotification(notification);

            using var package = new ExcelPackage();
            var worksheetModel = package.Workbook.Worksheets.Add("Modelo");

            CreateStyles(worksheetModel);
            AddWorksheetModel(worksheetModel, assessment);

            AddWorksheetsDepartments(package, assessment, assessmentModels);

            var fileName = $"modelo-avaliacao-mensal-{assessment.ReferenceDate.Value.GetFormatedDate().Replace("/", "-")}";
            var fileBytes = package.GetAsByteArray();

            await _excelService.SalvarExcel(fileName, fileBytes);

            _notificationsService.RemoveNotification(notification);
            _notificationsService.AddNotification($"O Relatório de Avaliação Mensal foi gerado com sucesso!");
        }
        catch (Exception ex)
        {
            _notificationsService.RemoveNotification(notification);

            var erroMessage = "Não foi possível exportar o Relatório de Avaliação Mensal! Contate o Suporte.";
            _notificationsService.AddNotification(erroMessage, Notification.NotificationType.Error);
            await _emailService.SendErrorToSupport(ex, erroMessage);
            return false;
        }
        finally
        {
            GC.Collect();
        }

        return true;
    }

    private static void AddWorksheetsDepartments(ExcelPackage package, MonthlyAssessment assessment, List<AssessmentModel> assessmentModels)
    {
        var departments = assessmentModels.Select(c => c.Department).ToList();

        foreach (var assessmentModel in assessmentModels)
        {
            var departmentName = assessmentModel.Department.Name;
            var assessmentCriterias = assessmentModel.AssessmentAspects.SelectMany(a => a.Criteria).ToList();

            if (!assessmentModel.AssessmentAspects.Any() || !assessmentModel.Employees.Any())
                return;

            var worksheetDepartment = package.Workbook.Worksheets.Add(departmentName);
            var totalColumns = assessmentCriterias.Count + (assessment.IsClosed ? 5 : 3);

            var firstRange = worksheetDepartment.Cells[1, 1, 1, totalColumns];
            firstRange.Merge = true;
            firstRange.Value = departmentName;
            firstRange.StyleName = "CustomTitle1";

            var currentRow = 2;
            var currentColumn = 1;

            for (int i = 0; i < 3; i++)
            {
                var cell = worksheetDepartment.Cells[currentRow, currentColumn + i];
                cell.Value = "*******";
                cell.StyleName = "CustomTitle2";
            }

            currentColumn += 3;

            foreach (var assessmentAspect in assessmentModel.AssessmentAspects)
            {
                var assessmentAspectRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn + (assessmentAspect.Criteria.Count-1)];
                assessmentAspectRange.Merge = true;
                assessmentAspectRange.Value = assessmentAspect.Name;
                assessmentAspectRange.StyleName = "CustomTitle2";

                currentColumn += assessmentAspect.Criteria.Count;
            }

            if (assessment.IsClosed)
            {
                var gradeRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn + 1];

                gradeRange.Value = "Resultado";
                gradeRange.Merge = true;
                gradeRange.StyleName = "CustomTitle2";
            }

            currentRow++;
            currentColumn = 1;

            var employeeRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
            employeeRange.Value = "Funcionário";
            employeeRange.StyleName = "CustomTitle3";
            employeeRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            employeeRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
            currentColumn++;

            var rgRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
            rgRange.Value = "RG";
            rgRange.StyleName = "CustomTitle3";
            rgRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            rgRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
            currentColumn++;

            var admissionDateRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
            admissionDateRange.Value = "Data de Admissão";
            admissionDateRange.StyleName = "CustomTitle3";
            admissionDateRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            admissionDateRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
            currentColumn++;

            foreach (var criteria in assessmentCriterias)
            {
                var criteriaRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                criteriaRange.Value = criteria.Name;
                criteriaRange.StyleName = "CustomTitle3";
                criteriaRange.AddComment($"Limite: {criteria.GetLimitValue()}");

                currentColumn++;
            }

            if (assessment.IsClosed)
            {
                var percentageRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                percentageRange.Value = "Desempenho";
                percentageRange.StyleName = "CustomTitle3";

                currentColumn++;
                var noteRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                noteRange.Value = "Nota Final";
                noteRange.StyleName = "CustomTitle3";
            }

            currentRow++;
            foreach (var employeeAssessment in assessmentModel.AssessmentEmployees)
            {
                currentColumn = 1;

                var employeeCriteriaRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                employeeCriteriaRange.Value = employeeAssessment.Employee.Name;
                employeeCriteriaRange.StyleName = "CustomText";
                currentColumn++;

                var rgCriteriaRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                rgCriteriaRange.Value = employeeAssessment.Employee.RG;
                rgCriteriaRange.StyleName = "CustomText";
                currentColumn++;

                var admissionDateCriteriaRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                admissionDateCriteriaRange.Value = employeeAssessment.Employee.AdmissionDate.Value.ToShortDateString();
                admissionDateCriteriaRange.StyleName = "CustomText";

                var employeeCriterias = employeeAssessment.AssessmentCollections.SelectMany(ac => ac.AssessmentAspects.SelectMany(aa => aa.Criteria)).ToList();
                foreach (var criteria in employeeCriterias)
                {
                    currentColumn++;

                    var criteriaRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                    criteriaRange.Value = criteria.GetValue();
                    criteriaRange.StyleName = "CustomText";

                    switch (criteria.CriteriaType)
                    {
                        case CriteriaType.Integer:
                            criteriaRange.Value = criteria.ValueCriteria.ValueInt;
                            criteriaRange.Style.Numberformat.Format = "0"; 
                            break;
                        case CriteriaType.Decimal:
                            criteriaRange.Value = criteria.ValueCriteria.ValueDecimal;
                            criteriaRange.Style.Numberformat.Format = "0.00";
                            break;
                        case CriteriaType.Percentage:
                            criteriaRange.Value = criteria.ValueCriteria.ValuePercentage;
                            criteriaRange.Style.Numberformat.Format = "0.00";
                            break;
                        case CriteriaType.Time:
                            criteriaRange.Value = criteria.ValueCriteria.ValueTime;
                            break;
                        default:
                            break;
                    }

                    criteriaRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    criteriaRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                }

                if (assessment.IsClosed)
                {
                    currentColumn++;
                    var percentageRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];

                    var percentage = employeeAssessment.AssessmentCollections.Sum(a => a.PercentageSum) / (employeeAssessment.AssessmentCollections.Sum(a => a.QtdCriterias)) / 100;

                    percentageRange.Value = percentage;
                    percentageRange.StyleName = "CustomText";
                    percentageRange.Style.Numberformat.Format = "0.00%";


                    currentColumn++;
                    var noteRange = worksheetDepartment.Cells[currentRow, currentColumn, currentRow, currentColumn];
                    noteRange.Value = Math.Round(employeeAssessment.Note, 2);
                    noteRange.StyleName = "CustomText";
                    noteRange.Style.Numberformat.Format = "0.00";
                }
                currentRow++;
            }

            int startRow = 3;
            int startColumn = 1;

            int endColumn = totalColumns;
            int endRow = currentRow - 1;

            var tableRange = worksheetDepartment.Cells[startRow, startColumn, endRow, endColumn];

            var table = worksheetDepartment.Tables.Add(tableRange, "Table_" + departmentName);
            table.ShowHeader = true;
            table.ShowFilter = true;
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Light9;

            if(assessment.IsClosed)
            {
                table.ShowTotal = true;
                worksheetDepartment.Cells[table.Address.End.Row, startColumn].Value = "Média Total";

                var percentageColumn = table.Columns["Desempenho"];
                var noteColumn = table.Columns["Nota Final"];
                if (percentageColumn != null)
                {
                    percentageColumn.TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Average;
                    percentageColumn.TotalsRowStyle.NumberFormat.Format = "0.00%";
                    percentageColumn.TotalsRowStyle.Alignment.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                if (noteColumn != null)
                {
                    noteColumn.TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Average;
                    noteColumn.TotalsRowStyle.NumberFormat.Format = "0.00";
                    noteColumn.TotalsRowStyle.Alignment.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

            }

            worksheetDepartment.Cells[worksheetDepartment.Dimension.Address].AutoFitColumns(60);
        }
    }

    private static void AddWorksheetModel(ExcelWorksheet worksheet, MonthlyAssessment assessment)
    {
        worksheet.Cells["A1:L1"].Merge = true;
        worksheet.Cells["A1:L1"].Value = "Modelo de Avaliação Mensal - RBI Papéis";
        worksheet.Cells["A1:L1"].StyleName = "CustomTitle1";

        worksheet.Cells["A2"].Value = "Responsável:";
        worksheet.Cells["A2"].Style.Font.Size = 11;
        worksheet.Cells["A2"].Style.Font.Bold = true;
        worksheet.Cells["A2"].Style.Font.Color.SetColor(darkBlue);

        worksheet.Cells["B2:L2"].Merge = true;
        worksheet.Cells["B2:L2"].Value = assessment.Responsible;
        worksheet.Cells["B2:L2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        worksheet.Cells["B2:L2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

        worksheet.Cells["A3"].Value = "Competência:";
        worksheet.Cells["A3"].Style.Font.Size = 11;
        worksheet.Cells["A3"].Style.Font.Bold = true;
        worksheet.Cells["A3"].Style.Font.Color.SetColor(darkBlue);

        worksheet.Cells["B3:L3"].Merge = true;
        worksheet.Cells["B3:L3"].Value = assessment.ReferenceDate.Value.GetFormatedDate();
        worksheet.Cells["B3:L3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        worksheet.Cells["B3:L3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

        worksheet.Cells["A4"].Value = "Descrição:";
        worksheet.Cells["A4"].Style.Font.Size = 11;
        worksheet.Cells["A4"].Style.Font.Bold = true;
        worksheet.Cells["A4"].Style.Font.Color.SetColor(darkBlue);

        worksheet.Cells["B4:L4"].Merge = true;
        worksheet.Cells["B4:L4"].Value = string.IsNullOrEmpty(assessment.Description) ? (assessment.IsClosed ? assessment.GetConfirmationMessage() : assessment.GetScratchMessage()) : assessment.Description;
        worksheet.Cells["B4:L4"].Style.WrapText = true; 
        worksheet.Cells["B4:L4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        worksheet.Cells["B4:L4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

        if (assessment.IsClosed)
        {
            worksheet.Cells["A5"].Value = "Concluido em:";
            worksheet.Cells["A5"].Style.Font.Size = 11;
            worksheet.Cells["A5"].Style.Font.Bold = true;
            worksheet.Cells["A5"].Style.Font.Color.SetColor(darkBlue);

            worksheet.Cells["B5:L5"].Merge = true;
            worksheet.Cells["B5:L5"].Value = assessment.ClosedDate.Value.ToShortDateString();
            worksheet.Cells["B5:L5"].Style.WrapText = true;
            worksheet.Cells["B5:L5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            worksheet.Cells["B5:L5"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
        }

        if (!assessment.IsClosed)
        {
            worksheet.Cells["A6"].Value = "Observações:";
            worksheet.Cells["A6"].Style.Font.Size = 11;
            worksheet.Cells["A6"].Style.Font.Bold = true;
            worksheet.Cells["A6"].Style.Font.Color.SetColor(darkBlue);

            worksheet.Cells["B6:L10"].Merge = true;
            worksheet.Cells["B6:L10"].Value = "Esse é um arquivo de Modelo para importação de Avaliação Mensal. \nPara sua correta leitura, não modifique os campos exportados e já preenchidos, modifique apenas\nas células que pertencem às avaliações de critérios. \n\nCampos não preenchidos terão o valor padrão";
            worksheet.Cells["B6:L10"].Style.WrapText = true;
            worksheet.Cells["B6:L10"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            worksheet.Cells["B6:L10"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
            worksheet.Cells["B6:L10"].AutoFitColumns();
        }

        worksheet.Column(1).AutoFit();
        worksheet.Column(2).AutoFit();
    }

    private static void CreateStyles(ExcelWorksheet worksheet)
    {
        var titleStyle = worksheet.Workbook.Styles.CreateNamedStyle("CustomTitle1");
        titleStyle.Style.Font.SetFromFont("Aptos Narrow", 15, bold: true);

        titleStyle.Style.Font.Color.SetColor(darkBlue);
        titleStyle.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
        titleStyle.Style.Border.Bottom.Color.SetColor(darkBlue);
        titleStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        titleStyle.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        var title2Style = worksheet.Workbook.Styles.CreateNamedStyle("CustomTitle2");
        title2Style.Style.Font.SetFromFont("Aptos Narrow", 11, bold: true);
        var lightBlue = System.Drawing.Color.FromArgb(68, 179, 225);

        title2Style.Style.Font.Color.SetColor(darkBlue);
        title2Style.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        title2Style.Style.Border.Left.Color.SetColor(lightBlue);
        title2Style.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        title2Style.Style.Border.Right.Color.SetColor(lightBlue);
        title2Style.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        title2Style.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;      

        var title3Style = worksheet.Workbook.Styles.CreateNamedStyle("CustomTitle3");
        title3Style.Style.Font.SetFromFont("Aptos Narrow", 11, bold: true);

        title3Style.Style.Font.Color.SetColor(System.Drawing.Color.White);
        title3Style.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        title3Style.Style.Border.Bottom.Color.SetColor(lightBlue);
        title3Style.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        title3Style.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;

        var textStyle = worksheet.Workbook.Styles.CreateNamedStyle("CustomText");
        textStyle.Style.Font.SetFromFont("Aptos Narrow", 11);

        textStyle.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        textStyle.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
    }

    public class ExportAssessmentModel
    {
        public string AspectId { get; set; }
        public string CriteriaId { get; set; }
        public int Col { get; set; }

        public ExportAssessmentModel(string aspectId, string criteriaId, int col)
        {
            AspectId = aspectId;
            CriteriaId = criteriaId;
            Col = col;
        }
    }
}