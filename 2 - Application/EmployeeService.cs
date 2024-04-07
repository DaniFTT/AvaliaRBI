using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;
using AvaliaRBI.Shared.Extensions;
using AvaliaRBI.Shared.Validation;
using OfficeOpenXml;
using System.Globalization;
using static AvaliaRBI._2___Application.Shared.Notification;

namespace AvaliaRBI._2___Application;

public class EmployeeService : BaseService<Employee>
{
    private EmployeeRepository _repository;
    private PositionRepository _positionRepository;
    private ExcelService _excelService;
    private EmailService _emailService;
    public EmployeeService(IBaseRepository<Employee> repository, IBaseRepository<PositionJob> positionJob,
        ExcelService excelService, NotificationsService notificationsService, EmailService emailService) : base(repository, notificationsService)
    {
        _repository = repository as EmployeeRepository;
        _positionRepository = positionJob as PositionRepository;
        _excelService = excelService;
        _emailService = emailService;
    }

    public async Task<IEnumerable<Employee>> GetAllByReferenceDate(DateTime referenceDate)
    {
        return await _repository.GetAllByReferenceDate(referenceDate);
    }

    public async Task<IEnumerable<Employee>> GetAllByAssessment(int[] ids)
    {
        return await _repository.GetAllByAssessment(ids);
    }

    public async Task<Employee> GetByCPF(string cpf)
    {
        return await _repository.GetByCPF(cpf);
    }

    string[] excelHeaders = { "Nome*", "CPF*", "Cargo*", "Departamento", "Setor", "Data de Admissão*" };

    ExcelField<EmployeeImportModel>[] HeaderFields = new ExcelField<EmployeeImportModel>[]  
    {
        new ExcelField<EmployeeImportModel>(1, "Nome")
            .WithRequiredRule()
            .WithMaxLengthRule(200)
            .SetAction((employee, value) => employee.Name = value),

        new ExcelField<EmployeeImportModel>(2, "CPF")
            .WithRequiredRule()
            .WithCPFRule()
            .SetAction((employee, value) => employee.CPF = value.NormalizeCPF()),

        new ExcelField<EmployeeImportModel>(3, "Cargo")
            .WithRequiredRule()
            .WithMaxLengthRule(200)
            .SetAction((employee, value) => employee.PositionName = value),

        new ExcelField<EmployeeImportModel>(4, "Departamento", comment: "Este campo não é obrigatório para a inserção no sistema (Será preechido a partir do Cargo Selecionado)."),
        new ExcelField<EmployeeImportModel>(5, "Setor", comment: "Este campo não é obrigatório para a inserção no sistema (Será preechido a partir do Cargo Selecionado)."),

        new ExcelField<EmployeeImportModel>(6, "Data de Admissão")
            .WithRequiredRule()
            .WithNoFutureDateRule()
            .SetAction((employee, value) => employee.AdmissionDate = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"))),
    };

    public async Task<bool> ExportEmployeesToExcel(string processId)
    {
        Notification notification = null;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        try
        {
            var employees = (await GetAll()).ToArray();

            notification = new Notification($"O Relatório de Funcionários está sendo gerado", Convert.ToDouble(employees.Length), processId);
            _notificationsService.AddNotification(notification);

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Funcionários");
            var currentRow = 1;

            foreach (var field in HeaderFields)
            {
                var name = field.Header;
                var cell = worksheet.Cells[currentRow, field.Col];
                cell.Value = name;

                if (!string.IsNullOrEmpty(field.Comment))
                    cell.AddComment(field.Comment);
            }

            foreach (var employee in employees)
            {
                currentRow++;
                worksheet.Cells[currentRow, 1].Value = employee.Name;
                worksheet.Cells[currentRow, 2].Value = employee.CPF;
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
            _notificationsService.AddNotification($"O Relatório de Funcionários foi gerado com sucesso!");
        }
        catch (Exception ex)
        {
            _notificationsService.RemoveNotification(notification);

            var erroMessage = "Não foi possível exportar o Relatório de Funcionários! Contate o Suporte.";
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

    public async Task<bool> ImportEmployeesByExcel(string fullPath, string processId)
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
                importModel.Title = "Erro ao importar funcionários";
                _notificationsService.AddNotification(new Notification(importModel));
                return false;
            }

            notification = new Notification($"A importação de Funcionários está sendo processada", Convert.ToDouble(totalRows), processId);
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
                    header.SetValueIfValid(employeeModel, value, worksheet.Name, row, importModel);
                }

                var position = positions.FirstOrDefault(p => employeeModel.PositionName.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (position == null)
                    importModel.AddNota(worksheet.Name,row, "O Cargo informado não foi encontrado!");

                if (importModel.ContainsErrorsByRow(row))
                    continue;
     
                var employeeByCPF = await GetByCPF(employeeModel.CPF);
                if (employeeByCPF != null)
                {
                    employeeByCPF.UpdateEmployee(employeeModel, position);

                    var updatedResult = await _repository.Update(employeeByCPF.Id, employeeByCPF);
                    if(updatedResult <= 0)
                    {
                        importModel.AddNota(worksheet.Name, row, "Ocorreu um erro ao atualizar esse funcionário, entre em contato com o suporte!");
                        continue;
                    }

                    importModel.UpdatedCount++;
                    continue;
                }      

                var employee = new Employee(employeeModel.Name, employeeModel.CPF, employeeModel.AdmissionDate, position);
                var insertedResult = await _repository.Insert(employee);
                if(insertedResult <= 0)
                {
                    importModel.AddNota(worksheet.Name, row, "Ocorreu um erro ao inserir esse funcionário, entre em contato com o suporte!");
                    continue;
                }

                importModel.InsertedCount++;
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

            var erroMessage = "Não foi possível importar os Funcionários! Contate o Suporte.";
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
}

