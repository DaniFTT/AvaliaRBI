using AvaliaRBI._3___Domain.Abstractions;
using Newtonsoft.Json;
using SQLite;

namespace AvaliaRBI._3___Domain;

[Table(nameof(MonthlyAssessment))]
public class MonthlyAssessment : BaseEntity
{
    [NotNull, MaxLength(200)]
    public string Responsible { get; set; }

    [NotNull]
    public DateTime? ReferenceDate { get; set; }

    [MaxLength(400)]
    public string Description { get; set; }

    [NotNull]
    public DateTime? CreatedDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    [NotNull]
    public bool IsClosed { get; set; }

    public int OrderByDepartment { get; set; }

    [Ignore]
    public List<AssessmentCollection> AssessmentCollections { get; set; }

    [NotNull]
    public string AssessmentCollectionJson { get; set; }

    public void UpdateAssessmentCollections()
    {
        AssessmentCollections = !string.IsNullOrEmpty(AssessmentCollectionJson) ? JsonConvert.DeserializeObject<List<AssessmentCollection>>(AssessmentCollectionJson) : new List<AssessmentCollection>();
    }

    public void UpdateAssessmentCollectionsJson()
    {
        AssessmentCollectionJson = AssessmentCollections != null ? JsonConvert.SerializeObject(AssessmentCollections) : string.Empty;
    }

    public MonthlyAssessment()
    {
        CreatedDate = DateTime.Now;
    }

    public MonthlyAssessment(MonthlyAssessment monthlyAssessment)
    {
        Id = monthlyAssessment.Id;
        Responsible = monthlyAssessment.Responsible;
        Description = monthlyAssessment.Description;
        ReferenceDate = monthlyAssessment.ReferenceDate;
        IsClosed = monthlyAssessment.IsClosed;
        AssessmentCollections = monthlyAssessment.AssessmentCollections;
        CreatedDate = monthlyAssessment.CreatedDate;
        ClosedDate = monthlyAssessment.ClosedDate;
        UpdateAssessmentCollectionsJson();
    }
}

