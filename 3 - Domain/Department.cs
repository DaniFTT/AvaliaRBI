using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI.Shared.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace AvaliaRBI._3___Domain;

[Table(nameof(Department))]
public class Department : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(800)]
    public string Description { get; set; }

    [ForeignKey(typeof(Sector))]
    public int? SectorId { get; set; }

    [ManyToOne]
    public Sector Sector { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<PositionJob> Positions { get; set; }

    public Department() 
    {
        Sector = new Sector();
    }

    public Department(string name, string description)
    {
        Name = name.NormalizeString();
        Description = description;
    }

    public Department(Department department)
    {
        Id = department.Id;
        Name = department.Name.NormalizeString();
        Description = department.Description;
        SectorId = department.SectorId;
        Sector = department.Sector;
    }
}

