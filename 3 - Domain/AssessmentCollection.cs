using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._3___Domain.Enum;
using AvaliaRBI._3___Domain.Models;
using AvaliaRBI.Shared.Extensions;
using Newtonsoft.Json;
using SQLite;

namespace AvaliaRBI._3___Domain;

[Table(nameof(AssessmentCollection))]
public class AssessmentCollection : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(400)]
    public string Description { get; set; }

    [NotNull]
    public DateTime? CreatedAt { get; set; }

    [NotNull]
    public string AssessmentAspectsJson { get; set; }

    [Ignore]
    public List<AssessmentAspect> AssessmentAspects { get; set; }

    [NotNull]
    public string DepartmentsJson { get; set; }

    [Ignore]
    public List<Department> Departments { get; set; }

    public double PercentageSum { get; set; }
    public int QtdCriterias { get; set; }

    public void UpdateAssessmentAspect()
    {
        AssessmentAspects = JsonConvert.DeserializeObject<List<AssessmentAspect>>(AssessmentAspectsJson);
    }

    public void UpdateDepartments()
    {
        Departments = JsonConvert.DeserializeObject<List<Department>>(DepartmentsJson);
    }

    public void UpdateDepartments(List<Department> updatedDepartments)
    {
        Departments = JsonConvert.DeserializeObject<List<Department>>(DepartmentsJson);
        for (int i = 0; i < Departments.Count; i++)
        {
            var department = Departments[i];

            var currentDepartment = updatedDepartments.FirstOrDefault(d => d.Id == department.Id);
            if (currentDepartment == null)
                continue;

            department.Name = currentDepartment.Name;
            department.Description = currentDepartment.Description;
        }
    }

    public void UpdateCriteriaPercentage()
    {
        PercentageSum = 0.0;
        QtdCriterias = 0;

        AssessmentAspects.ForEach(ac =>
        {
            ac.Criteria.ForEach(c =>
            {
                c.UpdateNote();

                PercentageSum += c.Performance;
                QtdCriterias++;
            });
        });
    }

    public AssessmentCollection() { }

    public AssessmentCollection(AssessmentCollection assessmentCollection, bool updateValues = false)
    {
        Id = assessmentCollection.Id;
        Name = assessmentCollection.Name.NormalizeString();
        Description = assessmentCollection.Description;
        CreatedAt = assessmentCollection.CreatedAt.HasValue && assessmentCollection.CreatedAt.Value != default ? assessmentCollection.CreatedAt.Value : DateTime.Now.Date;
        AssessmentAspects = assessmentCollection.AssessmentAspects != null ? assessmentCollection.AssessmentAspects.Select(a => new AssessmentAspect(a, updateValues)).ToList() : new List<AssessmentAspect>();
        Departments = assessmentCollection.Departments != null ? assessmentCollection.Departments.Select(a => new Department(a)).ToList() : new List<Department>();
        UpdateAssessmentsAspectsJson();
        UpdateDepartmentsJson();
    }

    public void UpdateAssessmentsAspectsJson()
    {
        if (Id == 0)
        {
            foreach (AssessmentAspect aspect in AssessmentAspects)
            {
                aspect.Id = Guid.NewGuid().ToString();
                foreach (var criteria in aspect.Criteria)
                {
                    criteria.Id = Guid.NewGuid().ToString();
                    criteria.AspectId = aspect.Id;
                }
            }
        }
        else
        {
            foreach (AssessmentAspect aspect in AssessmentAspects)
            {
                foreach (var criteria in aspect.Criteria)
                {
                    criteria.Id = string.IsNullOrEmpty(criteria.Id) ? Guid.NewGuid().ToString() : criteria.Id;
                    criteria.AspectId = aspect.Id;
                }
            }
        }

        AssessmentAspectsJson = AssessmentAspects != null ? JsonConvert.SerializeObject(AssessmentAspects) : string.Empty;
    }

    public void UpdateDepartmentsJson()
    {
        DepartmentsJson = Departments != null ? JsonConvert.SerializeObject(Departments) : string.Empty;
    }

    public void UpdateValue(AssessmentCollection assessmentCollection, AssessmentType assessmentType)
    {
        Id = assessmentCollection.Id;
        Name = assessmentCollection.Name.NormalizeString();
        Description = assessmentCollection.Description;

        for (int i = 0; i < assessmentCollection.AssessmentAspects.Count; i++)
        {
            var aspect = assessmentCollection.AssessmentAspects[i];

            var currentAspect = AssessmentAspects.FirstOrDefault(a => a.Id == aspect.Id);
            if (currentAspect == null)
            {
                AssessmentAspects.Add(new AssessmentAspect(aspect));
                continue;
            }

            currentAspect.Id = aspect.Id;
            currentAspect.Name = aspect.Name;
            currentAspect.Description = aspect.Description;
            currentAspect.AssesmentType = aspect.AssesmentType;

            for (int j = 0; j < aspect.Criteria.Count; j++)
            {
                var criteria = aspect.Criteria[j];

                var currentCriteria = currentAspect.Criteria.FirstOrDefault(c => c.Id == criteria.Id);
                if (currentCriteria == null)
                {
                    currentAspect.Criteria.Add(new AssessmentCriteria(criteria));
                    continue;
                }

                currentCriteria.Id = criteria.Id;
                currentCriteria.Name = criteria.Name;
                currentCriteria.CriteriaType = criteria.CriteriaType;
                currentCriteria.AspectId = criteria.AspectId;
                currentCriteria.LimitValueCriteria = criteria.LimitValueCriteria;
                currentCriteria.IsPositive = criteria.IsPositive;
            }

            currentAspect.Criteria.RemoveAll(c => !aspect.Criteria.Any(ac => ac.Id == c.Id));
        }

        AssessmentAspects.RemoveAll(aa => (!assessmentCollection.AssessmentAspects.Any(acaa => acaa.Id == aa.Id) || aa.AssesmentType != assessmentType));

        Departments = assessmentCollection.Departments != null ? assessmentCollection.Departments.Select(a => new Department(a)).ToList() : new List<Department>();
        UpdateAssessmentsAspectsJson();
        UpdateDepartmentsJson();
    }
}

