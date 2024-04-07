using AvaliaRBI._3___Domain.Abstractions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace AvaliaRBI._3___Domain;

[Table(nameof(Employee))]
public class Employee : BaseEntity
{
    [NotNull, MaxLength(200)]
    public string Name { get; set; }

    [NotNull, MaxLength(14)]
    public string CPF { get; set; }

    [MaxLength(16)]
    public string PhoneNumber { get; set; }

    [NotNull]
    public DateTime? AdmissionDate { get; set; }

    [ForeignKey(typeof(PositionJob))]
    public int? PositionId { get; set; }

    [ManyToOne]
    public PositionJob Position { get; set; }

    public Employee()
    {
        Position = new PositionJob();
    }

    public Employee(string name, string cpf, DateTime admissionDate, PositionJob position)
    {
        Name = name.Trim();
        CPF = cpf;
        AdmissionDate = admissionDate;
        PositionId = position.Id;
        Position = position;
    }

    public Employee(Employee employee)
    {
        Id = employee.Id;
        Name = employee.Name.Trim();
        AdmissionDate = employee.AdmissionDate;
        Position = employee.Position;
        PositionId = employee.PositionId;
        CPF = employee.CPF;
        PhoneNumber = employee.PhoneNumber;
    }

    public void UpdateEmployee(EmployeeImportModel employeeImport, PositionJob position)
    {
        Name = employeeImport.Name.Trim();
        AdmissionDate = employeeImport.AdmissionDate;
        Position = position;
        PositionId = position.Id;
    }
}



public class EmployeeImportModel
{
    public string Name { get; set; }
    public string CPF { get; set; }
    public string PositionName { get; set; }
    public DateTime AdmissionDate { get; set; }
}