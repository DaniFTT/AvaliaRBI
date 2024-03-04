using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI.Shared.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace AvaliaRBI._3___Domain;

public class Sector : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(800)]
    public string Description { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<PositionJob> Positions { get; set; }

    public Sector() { }

    public Sector(string name, string description)
    {
        Name = name.NormalizeString();
        Description = description;
    }

    public Sector(Sector department)
    {
        Id = department.Id;
        Name = department.Name.NormalizeString();
        Description = department.Description;
    }
}

