using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;
using MudBlazor;
using OfficeOpenXml;
using System.ComponentModel;
using System.Diagnostics;
using static AvaliaRBI._2___Application.Shared.Notification;

namespace AvaliaRBI._2___Application;

public class EmployeeService : BaseService<Employee>
{
    private EmployeeRepository _repository;
    private PositionRepository _positionRepository;
    private PositionService _positionService;
    private ExcelService _excelService;
    private NotificationsService _notificationsService;
    public EmployeeService(IBaseRepository<Employee> repository, IBaseRepository<PositionJob> positionJob,
        ExcelService excelService, NotificationsService notificationsService, EmailService emailService) : base(repository, emailService)
    {
        _repository = repository as EmployeeRepository;
        _positionRepository = positionJob as PositionRepository;
        _excelService = excelService;
        _notificationsService = notificationsService;
    }

    public async Task<IEnumerable<Employee>> GetAllByReferenceDate(DateTime referenceDate)
    {
        return await _repository.GetAllByReferenceDate(referenceDate);
    }

    public async Task<IEnumerable<Employee>> GetAllByAssessment(int[] ids)
    {
        return await _repository.GetAllByAssessment(ids);
    }
    public async Task<Employee> GetByRG(string rg)
    {
        return await _repository.GetByRG(rg);
    }

    string[] excelHeaders = { "Nome*", "RG*", "Cargo*", "Departamento", "Setor", "Data de Admissão*" };

    public async Task ExportEmployeesToExcel(string processId)
    {
        try
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var employees = (await GetAll()).ToArray();

            var notification = new Notification($"O Relatório em Excel de Funcionários está sendo gerado", Convert.ToDouble(employees.Length), processId);
            _notificationsService.AddNotification(notification);

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Funcionários");
            var currentRow = 1;

            for (int i = 0; i < excelHeaders.Length; i++)
            {
                var name = excelHeaders[i];
                var cell = worksheet.Cells[currentRow, i + 1];
                cell.Value = name;

                if (name.Contains("*"))
                    cell.AddComment("Este campo é obrigatório para a inserção no sistema.", "Sistema");

                if(name == "Departamento" || name == "Setor")
                    cell.AddComment("Este campo não é obrigatório para a inserção no sistema (Será preechido a partir do Cargo Selecionado).", "Sistema");
            }

            foreach (var employee in employees)
            {
                currentRow++;
                worksheet.Cells[currentRow, 1].Value = employee.Name;
                worksheet.Cells[currentRow, 2].Value = employee.RG;
                worksheet.Cells[currentRow, 3].Value = employee.Position.Name;
                worksheet.Cells[currentRow, 4].Value = employee.Position.Department.Name;
                worksheet.Cells[currentRow, 5].Value = employee.Position.Department.Sector.Name;
                worksheet.Cells[currentRow, 6].Value = employee.AdmissionDate?.ToString("dd/MM/yyyy");

                _notificationsService.UpdateProgressNotification(notification, (currentRow - 1));
            }

            var range = worksheet.Cells[1, 1, ++currentRow, excelHeaders.Length];
            var table = worksheet.Tables.Add(range, "FuncionariosTable");
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Light9;

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileName = $"relatorio-funcionarios-{DateTime.Now.ToString("dd-MM-yyyy")}";
            var fileBytes = package.GetAsByteArray();

            await _excelService.SalvarExcel(fileName, fileBytes);

            _notificationsService.RemoveNotification(notification);
            _notificationsService.AddNotification($"O Relatório em Excel de Funcionários foi gerado com sucesso!");
        }
        catch (Exception e)
        {
            _notificationsService.AddNotification("Não foi possível exportar o Relatório de Funcionários! Contate o Suporte.");
        }
    }

    public async Task ImportEmployeesByExcel(string fullPath, string processId)
    {
        var tempFilePath = Path.GetTempPath() + Path.GetFileName(fullPath);
        Notification notification = null;
        try
        {
            File.Copy(fullPath, tempFilePath, true);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(new FileInfo(fullPath));
            var worksheet = package.Workbook.Worksheets[0];

            int startRow = 2;
            int totalRows = worksheet.Dimension.End.Row;

            notification = new Notification($"A importação de Funcionários está sendo processada", Convert.ToDouble(totalRows), processId);
            _notificationsService.AddNotification(notification);

            var importModel = new ImportDataModel(Path.GetFileName(fullPath));

            var positions = (await _positionRepository.GetAll()).ToList();
            for (int row = startRow; row <= totalRows; row++)
            {
                if(string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) || string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                    continue;

                importModel.ProcessedCount++;
                _notificationsService.UpdateProgressNotification(notification, (row - 1));

                string name = worksheet.Cells[row, Array.IndexOf(excelHeaders, "Nome*") + 1].Text;
                if (string.IsNullOrEmpty(name))
                    importModel.AddNota(row.ToString(), "Nome não informado.", NotaType.Error);
                if(name.Length > 200)
                    importModel.AddNota(row.ToString(), "Nome deve conter no máximo 200 caracteres.", NotaType.Error);

                string rg = worksheet.Cells[row, Array.IndexOf(excelHeaders, "RG*") + 1].Text;
                if (string.IsNullOrEmpty(rg))
                    importModel.AddNota(row.ToString(), "RG não informado.", NotaType.Error);

                string positionName = worksheet.Cells[row, Array.IndexOf(excelHeaders, "Cargo*") + 1].Text;
                if (string.IsNullOrEmpty(positionName))
                    importModel.AddNota(row.ToString(), "Cargo não informado.", NotaType.Error);

                string admissionDate = worksheet.Cells[row, Array.IndexOf(excelHeaders, "Data de Admissão*") + 1].Text;
                if (string.IsNullOrEmpty(admissionDate))
                    importModel.AddNota(row.ToString(), "Data de Admissão não informado.", NotaType.Error);

                var position = positions.FirstOrDefault(p => positionName.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                var date = DateTime.TryParse(admissionDate, out var dateValue) ? dateValue : (DateTime?)null;

                var getEmployeeByRg = await GetByRG(rg);
                if (getEmployeeByRg != null)
                    importModel.AddNota(row.ToString(), "Esse RG já pertence a um Funcionário.", NotaType.Error);
                
                if (importModel.ContainsErrorsByRow(row))
                    continue;

                var employee = new Employee(name, rg, date.Value, position);
                await _repository.Insert(employee);
                importModel.InsertedCount++;
                importModel.UpdatedCount++;
            }

            _notificationsService.RemoveNotification(notification);

            importModel.Title = importModel.ContainsErrors ? "Não foi possível inserir todos os funcionários. Verifique os detalhes da Importação!" : 
                "Importação de Funcionários Realizada com Sucesso! Veja os detalhes";
    
            var notificationImport = new Notification(importModel);
           _notificationsService.AddNotification(notificationImport);
        }
        catch (Exception ex)
        {
            if (notification != null)
                _notificationsService.RemoveNotification(notification);

            _notificationsService.AddNotification("Não foi possível exportar o Relatório de Funcionários! Contate o Suporte.");
        }
    }
}

