using AvaliaRBI._3___Domain.Enum;

namespace AvaliaRBI._3___Domain.Models;

public class AssessmentAspect
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string Description { get; set; }
    public AssessmentType? AssesmentType { get; set; }

    public List<AssessmentCriteria> Criteria { get; set; }

    public AssessmentAspect()
    {
        Criteria = new List<AssessmentCriteria>();
    }

    public AssessmentAspect(string name, string description, List<AssessmentCriteria> criteria, AssessmentType type)
    {
        Name = name;
        Description = description;
        Criteria = criteria ?? new List<AssessmentCriteria>();
        AssesmentType = type;
    }

    public AssessmentAspect(AssessmentAspect assessmentAspect, bool updateValue = true)
    {
        Id = assessmentAspect.Id;
        Name = assessmentAspect.Name;
        Description = assessmentAspect.Description;
        AssesmentType = assessmentAspect.AssesmentType;
        Criteria = assessmentAspect.Criteria != null ? assessmentAspect.Criteria.Select(c => new AssessmentCriteria(c, updateValue)).ToList() : new List<AssessmentCriteria>();
    }
}
