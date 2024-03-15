using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._3___Domain.Enum;

namespace AvaliaRBI._4___Repository;

public class AssessmentEmployeeRepository : BaseRepository<AssessmentEmployee>
{
    public PositionRepository PositionRepository { get; set; }
    public AssessmentEmployeeRepository(IBaseRepository<PositionJob> positionRepository) : base()
    {
        PositionRepository = positionRepository as PositionRepository;
    }


    public async Task<List<AssessmentEmployee>> GetAssessmentEmployeesByAssessmentId(int assessmentId, AssessmentType assessmentType, bool updateValues = false)
    {
        List<AssessmentCollection> collections = new List<AssessmentCollection>();
        List<Department> departments = new List<Department>();
        List<PositionJob> positions = new List<PositionJob>();
        await SetUpDb();

        var assessmentsEmployees = await _connection.Table<AssessmentEmployee>()
            .Where(ae => ae.AssessmentId == assessmentId)
            .ToListAsync();


        if (updateValues)
        {
            collections = (await _connection.Table<AssessmentCollection>().ToListAsync());
            positions = (await PositionRepository.GetAll()).ToList();
            departments = positions.Select(e => e.Department).ToList();
        }

        foreach (var assessmentsEmployee in assessmentsEmployees)
        {
            assessmentsEmployee.UpdateAssessmentCollections();
            assessmentsEmployee.UpdateEmployee();

            if (updateValues)
            {
                assessmentsEmployee.Employee.Position = positions.FirstOrDefault(p => p.Id == assessmentsEmployee.Employee.PositionId);
                foreach (var col in collections)
                {
                    col.UpdateDepartments(departments);
                    col.UpdateAssessmentAspect();

                    var hasCollection = assessmentsEmployee.AssessmentCollections.Any(ac => ac.Id == col.Id);
                    var shouldHaveCollection = col.Departments.Any(d => d.Id == assessmentsEmployee.Employee.Position.Department.Id);

                    if (!hasCollection && !shouldHaveCollection)
                        continue;

                    if (!hasCollection && shouldHaveCollection)
                    {
                        assessmentsEmployee.AssessmentCollections.Add(new AssessmentCollection(col, true));
                        continue;
                    }

                    if (hasCollection && !shouldHaveCollection)
                    {
                        assessmentsEmployee.AssessmentCollections.RemoveAll(ac => ac.Id == col.Id);
                        continue;
                    }

                    assessmentsEmployee.AssessmentCollections.FirstOrDefault(ac => ac.Id == col.Id).UpdateValue(col, assessmentType);
                }

                assessmentsEmployee.AssessmentCollections.RemoveAll(ac => !collections.Any(c => c.Id == ac.Id));
            }

        }

        return assessmentsEmployees;
    }

    public async Task<AssessmentEmployee> GetAssessmentEmployeeByAssessmentAndEmployeeId(int assessmentId, int employeeId)
    {
        await SetUpDb();

        var assessmentsEmployee = await _connection.Table<AssessmentEmployee>().FirstOrDefaultAsync(ae => ae.AssessmentId == assessmentId && ae.EmployeeId == employeeId);

        assessmentsEmployee.UpdateAssessmentCollections();
        assessmentsEmployee.UpdateEmployee();

        return assessmentsEmployee;
    }
    public async Task<AssessmentEmployee[]> GetLastTreeAssessmentEmployee(DateTime referenceDate, int employeeId)
    {
        await SetUpDb();

        var lastAssessments = (await _connection.Table<MonthlyAssessment>().Where(ma => ma.IsClosed).ToArrayAsync())
            .Where(x => x.ReferenceDate < referenceDate.Date).OrderBy(x => x.ReferenceDate.Value).Take(3).ToArray();

        var assessmentsEmployees = await _connection.Table<AssessmentEmployee>().Where(ae => ae.EmployeeId == employeeId).ToArrayAsync();
        assessmentsEmployees = assessmentsEmployees.Where(ae => lastAssessments.Any(la => la.Id == ae.AssessmentId)).ToArray();

        foreach (var assessmentEmployee in assessmentsEmployees)
        {
            assessmentEmployee?.UpdateAssessmentCollections();
            assessmentEmployee?.UpdateEmployee();
        }

        return assessmentsEmployees;
    }

    public override async Task<IEnumerable<AssessmentEmployee>> GetAll()
    {
        await SetUpDb();

        var assessments = await base.GetAll();
        foreach (var assessment in assessments)
        {
            assessment?.UpdateAssessmentCollections();
            assessment?.UpdateEmployee();
        }

        return assessments;
    }

    public override async Task<AssessmentEmployee> GetById(int id)
    {
        await SetUpDb();

        var assessment = await base.GetById(id);
        assessment?.UpdateAssessmentCollections();
        assessment?.UpdateEmployee();

        return assessment;
    }

    public override Task<int> Insert(AssessmentEmployee value)
    {
        value?.UpdateAssessmentCollectionsJson();
        value?.UpdateEmployeeJson();

        return base.Insert(value);
    }

    public override Task<int> Update(int id, AssessmentEmployee value)
    {
        value?.UpdateAssessmentCollectionsJson();
        value?.UpdateEmployeeJson();

        return base.Update(id, value);
    }
}

