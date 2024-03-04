using AvaliaRBI._3___Domain.Enum;

namespace AvaliaRBI._3___Domain.Models;

public class AssessmentModel
{
    public Department Department { get; set; }
    public AssessmentType AssessmentType { get; set; }

    public List<AssessmentCollection> AssessmentCollections { get; set; }

    public List<AssessmentEmployee> AssessmentEmployees { get; set; }

    public List<AssessmentAspect> AssessmentAspects { get => AssessmentCollections.SelectMany(ac => ac.AssessmentAspects).Where(aa => aa.AssesmentType == AssessmentType).ToList(); }

    public List<Employee> Employees { get => AssessmentEmployees.Select(ac => ac.Employee).ToList(); }
}

