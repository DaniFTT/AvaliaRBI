using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;
using MudBlazor;
using OfficeOpenXml;
using System.ComponentModel;

namespace AvaliaRBI._2___Application;

public class EmployeeService : BaseService<Employee>
{
    private EmployeeRepository _repository;
    private PositionService _positionService;
    private ExcelService _excelService;
    private NotificationsService _notificationsService;
    public EmployeeService(IBaseRepository<Employee> repository, ExcelService excelService, NotificationsService notificationsService) : base(repository)
    {
        _repository = new EmployeeRepository();
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
    public async Task ExportEmployeesToExcel(string processId)
    {
        try
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var employees = (await GetAll()).ToArray();

            var notification = new Notification($"O Relatório em Excel de Funcionários está sendo gerado", Convert.ToDouble(employees.Length), processId);
            _notificationsService.AddProgressNotification(notification);

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Funcionários");
            var currentRow = 1;

            string[] headers = { "Nome*", "RG*", "Cargo*", "Departamento", "Setor", "Data de Admissão*" };

            for (int i = 0; i < headers.Length; i++)
            {
                var name = headers[i];
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

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var range = worksheet.Cells[1, 1, ++currentRow, headers.Length];
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
            throw;
        }
       
    }
}

