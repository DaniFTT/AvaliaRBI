using AvaliaRBI._3___Domain;
using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._4___Repository;
public class AssessmentCollectionRepository : BaseRepository<AssessmentCollection>
{
    public AssessmentCollectionRepository() : base() { }


    public override async Task<IEnumerable<AssessmentCollection>> GetAll()
    {
        await SetUpDb();

        var collections = await base.GetAll();
        var departments = await _connection.Table<Department>().ToListAsync();

        foreach (var collection in collections)
        {
            collection?.UpdateAssessmentAspect();
            collection?.UpdateDepartments(departments);
        }

        return collections;
    }

    public override async Task<AssessmentCollection> GetById(int id)
    {
        await SetUpDb();

        var collection = await base.GetById(id);

        var departments = await _connection.Table<Department>().ToListAsync();

        collection?.UpdateAssessmentAspect();
        collection?.UpdateDepartments(departments);

        return collection;
    }

    public override async Task<int> Insert(AssessmentCollection value)
    {
        value?.UpdateAssessmentsAspectsJson();
        value?.UpdateDepartmentsJson();

        return await base.Insert(value);
    }

    public override async Task<int> Update(int id, AssessmentCollection value)
    {
        value?.UpdateAssessmentsAspectsJson();
        value?.UpdateDepartmentsJson();

        return await base.Update(id, value);
    }

    public async Task<AssessmentCollection> GetByName(string name)
    {
        await SetUpDb();
        name = name.NormalizeString();
        return await _connection.Table<AssessmentCollection>().FirstOrDefaultAsync(x => x.Name == name);
    }
}

