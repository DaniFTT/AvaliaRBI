using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI.Shared.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace AvaliaRBI._3___Domain;

[Table(nameof(PositionJob))]
public class PositionJob : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(800)]
    public string Description { get; set; }

    [ForeignKey(typeof(Department))]
    public int? DepartmentId { get; set; }

    [ManyToOne]
    public Department Department { get; set; }

    public PositionJob()
    {
        Department = new Department();
    }

    public PositionJob(string name, string description, Department department)
    {
        Name = name.NormalizeString();
        Description = description;
        DepartmentId = department.Id;
        Department = department;
    }

    public PositionJob(PositionJob position)
    {
        Id = position.Id;
        Name = position.Name.NormalizeString();
        Description = position.Description;
        DepartmentId = position.DepartmentId;
        Department = position.Department;
    }
}

