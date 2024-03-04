using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._3___Domain.Enum;
using Newtonsoft.Json;
using SQLite;

namespace AvaliaRBI._3___Domain;

[Table(nameof(AssessmentEmployee))]
public class AssessmentEmployee : BaseEntity
{
    [Indexed, NotNull]
    public int EmployeeId { get; set; }

    [Indexed, NotNull]
    public int AssessmentId { get; set; }

    [NotNull]
    public AssessmentType AssesmentType { get; set; }

    [Ignore]
    public List<AssessmentCollection> AssessmentCollections { get; set; }

    [NotNull]
    public string AssessmentCollectionsJson { get; set; }

    [NotNull]
    public double Note { get; set; }

    [Ignore]
    public Employee Employee { get; set; }

    [NotNull]
    public string EmployeeJson { get; set; }

    public void UpdateAssessmentCollections()
    {
        AssessmentCollections = !string.IsNullOrEmpty(AssessmentCollectionsJson) ? JsonConvert.DeserializeObject<List<AssessmentCollection>>(AssessmentCollectionsJson) : new List<AssessmentCollection>();
    }

    public void UpdateAssessmentCollectionsJson()
    {
        if (AssessmentCollections != null && AssessmentCollections.Any())
        {
            var percentageSum = 0.0;
            var qtdCriterias = 0;
            foreach (var assessmentCollection in AssessmentCollections)
            {
                assessmentCollection.UpdateCriteriaPercentage();
                percentageSum += assessmentCollection.PercentageSum;
                qtdCriterias += assessmentCollection.QtdCriterias;
            }

            Note = Math.Round((((percentageSum / 100) / qtdCriterias) * 5), 2);

            AssessmentCollectionsJson = JsonConvert.SerializeObject(AssessmentCollections);
            return;
        }

        AssessmentCollectionsJson = string.Empty;
    }

    public void UpdateEmployee()
    {
        Employee = !string.IsNullOrEmpty(EmployeeJson) ? JsonConvert.DeserializeObject<Employee>(EmployeeJson) : null;
    }

    public void UpdateEmployeeJson()
    {
        EmployeeJson = Employee != null ? JsonConvert.SerializeObject(Employee) : string.Empty;
    }

    public AssessmentEmployee() { }

    public AssessmentEmployee(Employee employee, List<AssessmentCollection> collections, bool updateValues = true)
    {
        EmployeeId = employee.Id;
        Employee = employee;
        AssessmentCollections = collections.Where(c => c.Departments.Any(d => d.Id == employee.Position.DepartmentId)).Select(c => new AssessmentCollection(c, updateValues)).ToList();
    }

    public AssessmentEmployee(AssessmentEmployee assessmentEmployee)
    {
        Id = assessmentEmployee.Id;
        AssessmentId = assessmentEmployee.AssessmentId;
        EmployeeId = assessmentEmployee.EmployeeId;
        Employee = assessmentEmployee.Employee;
        AssesmentType = assessmentEmployee.AssesmentType;
        AssessmentCollections = assessmentEmployee.AssessmentCollections;
        UpdateAssessmentCollectionsJson();
        UpdateEmployeeJson();
    }
}


