using AvaliaRBI._3___Domain.Abstractions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace AvaliaRBI._3___Domain;

[Table(nameof(Employee))]
public class Employee : BaseEntity
{
    [NotNull, MaxLength(200)]
    public string Name { get; set; }

    [NotNull, MaxLength(12)]
    public string RG { get; set; }

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

    public Employee(string name, string rg, DateTime admissionDate, PositionJob position)
    {
        Name = name.Trim();
        RG = rg;
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
        RG = employee.RG;
        PhoneNumber = employee.PhoneNumber;
    }
}
