using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Enum;

namespace AvaliaRBI._4___Repository;

public class MonthlyAssessmentRepository : BaseRepository<MonthlyAssessment>
{
    public MonthlyAssessmentRepository() : base() { }

    public override async Task<int> Insert(MonthlyAssessment value)
    {
        value?.UpdateAssessmentCollectionsJson();
        value.CreatedDate = DateTime.Now.Date;

        return await base.Insert(value);
    }

    public override async Task<IEnumerable<MonthlyAssessment>> GetAll()
    {
        var assessments = await base.GetAll();
        foreach (var assessment in assessments)
            assessment?.UpdateAssessmentCollections();

        return assessments.OrderByDescending(a => a.ReferenceDate.Value);
    }

    public override async Task<MonthlyAssessment> GetById(int id)
    {
        var assessment = await base.GetById(id);

        assessment?.UpdateAssessmentCollections();

        return assessment;
    }

    public async Task<MonthlyAssessment> GetByIdUpdated(int id)
    {
        var assessment = await base.GetById(id);

        assessment?.UpdateAssessmentCollections();

        var collections = await _connection.Table<AssessmentCollection>().ToListAsync();
        var departments = await _connection.Table<Department>().ToListAsync();

        for (int i = 0; i < assessment.AssessmentCollections.Count; i++)
        {
            var collection = assessment.AssessmentCollections[i];
            var currentCollection = collections.FirstOrDefault(c => c.Id == collection.Id);

            if (currentCollection == null)
            {
                collection = null;
                continue;
            }

            currentCollection?.UpdateDepartments(departments);
            currentCollection?.UpdateAssessmentAspect();

            collection?.UpdateValue(currentCollection, AssessmentType.MonthlyAssessment);
        }

        assessment.AssessmentCollections.RemoveAll(ac => ac == null);

        return assessment;
    }


    public async Task<IEnumerable<MonthlyAssessment>> GetByReferenceDate(DateTime referenceDate)
    {
        await SetUpDb();

        var assessments = (await _connection.Table<MonthlyAssessment>().Where(x => x.IsClosed).ToListAsync())
            .Where(x => x.ReferenceDate >= referenceDate.Date && x.ReferenceDate < referenceDate.AddDays(1).Date).ToList();

        foreach (var assessment in assessments)
            assessment?.UpdateAssessmentCollections();

        return assessments;
    }

    public override async Task<int> Delete(int id)
    {
        await SetUpDb();

        var assessmentsEmployees = await _connection.Table<AssessmentEmployee>()
                        .Where(ae => ae.AssessmentId == id)
                        .ToListAsync();

        foreach (var assessment in assessmentsEmployees)
            await _connection.DeleteAsync(assessment);

        return await base.Delete(id);
    }
}
