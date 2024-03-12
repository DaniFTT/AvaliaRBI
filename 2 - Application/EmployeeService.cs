using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;
using AvaliaRBI.Shared.Validation;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using MudBlazor;
using OfficeOpenXml;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

    string[] excelHeaders = { "Nome*", "RG*", "Cargo*", "Departamento", "Setor", "Data de Admiss�o*" };

    ExcelField<EmployeeImportModel>[] HeaderFields = new ExcelField<EmployeeImportModel>[]  
    {
        new ExcelField<EmployeeImportModel>(1, "Nome*", "Nome")
            .WithRequiredRule()
            .WithMaxLengthRule(200)
            .SetAction((employee, value) => employee.Name = value),

        new ExcelField<EmployeeImportModel>(2, "RG*", "RG")
            .WithRequiredRule()
            .WithMaxLengthRule(12)
            .SetAction((employee, value) => employee.RG = value),

        new ExcelField<EmployeeImportModel>(3, "Cargo*", "Cargo")
            .WithRequiredRule()
            .WithMaxLengthRule(200)
            .SetAction((employee, value) => employee.PositionName = value),

        new ExcelField<EmployeeImportModel>(4, "Departamento"),
        new ExcelField<EmployeeImportModel>(5, "Setor"),

        new ExcelField<EmployeeImportModel>(6, "Data de Admiss�o*", "Data de Admiss�o")
            .WithRequiredRule()
            .WithMaxLengthRule(10)
            .SetAction((employee, value) => employee.AdmissionDate = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"))),
    };

    public async Task ExportEmployeesToExcel(string processId)
    {
        try
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var employees = (await GetAll()).ToArray();

            var notification = new Notification($"O Relat�rio em Excel de Funcion�rios est� sendo gerado", Convert.ToDouble(employees.Length), processId);
            _notificationsService.AddNotification(notification);

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Funcion�rios");
            var currentRow = 1;

            foreach (var field in HeaderFields)
            {
                var name = field.Header;
                var cell = worksheet.Cells[currentRow, field.Col];
                cell.Value = name;

                if (name.Contains("*"))
                    cell.AddComment("Este campo � obrigat�rio para a inser��o no sistema.", "Sistema");

                if (name == "Departamento" || name == "Setor")
                    cell.AddComment("Este campo n�o � obrigat�rio para a inser��o no sistema (Ser� preechido a partir do Cargo Selecionado).", "Sistema");
            }

            //for (int i = 0; i < excelHeaders.Length; i++)
            //{
            //    var name = excelHeaders[i];
            //    var cell = worksheet.Cells[currentRow, i + 1];
            //    cell.Value = name;

            //    if (name.Contains("*"))
            //        cell.AddComment("Este campo � obrigat�rio para a inser��o no sistema.", "Sistema");

            //    if(name == "Departamento" || name == "Setor")
            //        cell.AddComment("Este campo n�o � obrigat�rio para a inser��o no sistema (Ser� preechido a partir do Cargo Selecionado).", "Sistema");
            //}

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
            _notificationsService.AddNotification($"O Relat�rio em Excel de Funcion�rios foi gerado com sucesso!");
        }
        catch (Exception ex)
        {
            var erroMessage = "N�o foi poss�vel exportar o Relat�rio de Funcion�rios! Contate o Suporte.";
            _notificationsService.AddNotification(erroMessage);
            await _emailService.SendErrorToSupport(ex, erroMessage);
        }
    }

    public async Task ImportEmployeesByExcel(string fullPath, string processId)
    {
        Notification notification = null;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        int startRow = 2;

        try
        {
            using var package = new ExcelPackage(new FileInfo(fullPath));
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            int totalRows = worksheet.Dimension.End.Row;

            var importModel = new ImportNotificationModel(Path.GetFileName(fullPath));
            if (totalRows == 0)
            {
                importModel.AddNota(0, "Nenhum registro encontrado!");
                importModel.Title = "Erro ao importar funcion�rios";
                _notificationsService.AddNotification(new Notification(importModel));
                return;
            }

            notification = new Notification($"A importa��o de Funcion�rios est� sendo processada", Convert.ToDouble(totalRows), processId);
            _notificationsService.AddNotification(notification);

            var positions = (await _positionRepository.GetAll()).ToList();

            var employeeModel = new EmployeeImportModel();
            for (int row = startRow; row <= totalRows; row++)
            {           
                importModel.ProcessedCount++;
                _notificationsService.UpdateProgressNotification(notification, (row - 1));

                var cellRange = worksheet.Cells[row, 1, row, 6];
                if (cellRange.All(c => c.Value == null || string.IsNullOrWhiteSpace(c.Value.ToString())))
                    continue;

                foreach (var header in HeaderFields)
                {
                    var value = worksheet.Cells.GetString(row, header.Col);
                    header.SetValueIfValid(employeeModel, value, row, importModel);
                }

                var position = positions.FirstOrDefault(p => employeeModel.PositionName.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (position == null)
                    importModel.AddNota(row, "O Cargo informado n�o foi encontrado!");

                if (importModel.ContainsErrorsByRow(row))
                    continue;
     
                var employeeByRG = await GetByRG(employeeModel.RG);
                if (employeeByRG != null)
                {
                    employeeByRG.UpdateEmployee(employeeModel, position);

                    var updatedResult = await _repository.Update(employeeByRG.Id, employeeByRG);
                    if(updatedResult <= 0)
                    {
                        importModel.AddNota(row, "Ocorreu um erro ao atualizar esse funcion�rio, entre em contato com o suporte!");
                        continue;
                    }

                    importModel.UpdatedCount++;
                    continue;
                }      

                var employee = new Employee(employeeModel.Name, employeeModel.RG, employeeModel.AdmissionDate, position);
                var insertedResult = await _repository.Insert(employee);
                if(insertedResult <= 0)
                {
                    importModel.AddNota(row, "Ocorreu um erro ao inserir esse funcion�rio, entre em contato com o suporte!");
                    continue;
                }

                importModel.InsertedCount++;
            }

            _notificationsService.RemoveNotification(notification);

            importModel.Title = importModel.ContainsErrors ? "N�o foi poss�vel inserir todos os funcion�rios. Verifique os detalhes da Importa��o!" : 
                "Importa��o de Funcion�rios Realizada com Sucesso! Veja os detalhes";
    
            var notificationImport = new Notification(importModel);
           _notificationsService.AddNotification(notificationImport);
        }
        catch (Exception ex)
        {
            if (notification != null)
                _notificationsService.RemoveNotification(notification);

            var erroMessage = "N�o foi poss�vel importar os Funcion�rios! Contate o Suporte.";
            _notificationsService.AddNotification(erroMessage);
            await _emailService.SendErrorToSupport(ex, erroMessage);
        }
    }
}

