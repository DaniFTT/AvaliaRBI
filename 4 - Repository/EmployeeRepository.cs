using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;

namespace AvaliaRBI._4___Repository;

public class EmployeeRepository : BaseRepository<Employee>
{
    private PositionRepository _positionRepository;

    public EmployeeRepository(IBaseRepository<PositionJob> positionRepository) : base()
    {
        _positionRepository = positionRepository as PositionRepository;
    }

    public override async Task<IEnumerable<Employee>> GetAll()
    {
        await SetUpDb();

        var employees = (await base.GetAll()).ToArray();
        if (!employees.Any())
            return Array.Empty<Employee>();

        await IncludePositionObj(employees);

        return employees;
    }

    private async Task IncludePositionObj(IEnumerable<Employee> employees)
    {
        var positions = (await _positionRepository.GetAll()).Where(p => employees.Select(p => p.PositionId).Contains(p.Id)).ToArray();

        foreach (var employee in employees)
            employee.Position = positions.FirstOrDefault(d => d.Id == employee.PositionId);
    }

    public async Task<IEnumerable<Employee>> GetAllByReferenceDate(DateTime referenceDate)
    {
        await SetUpDb();

        referenceDate = referenceDate.AddMonths(1).Date;
        var employees = await _connection.Table<Employee>().Where(x => x.AdmissionDate < referenceDate).ToListAsync();
        await IncludePositionObj(employees);

        return employees;
    }

    public async Task<IEnumerable<Employee>> GetAllByAssessment(int[] ids)
    {
        await SetUpDb();

        var employees = (await _connection.Table<Employee>().Where(x => ids.Contains(x.Id)).ToListAsync());
        await IncludePositionObj(employees);

        return employees;
    }

    public override async Task<Employee> GetById(int id)
    {
        await SetUpDb();

        var employee = await base.GetById(id);
        if (employee != null)
            employee.Position = await _positionRepository.GetById(employee.PositionId.Value);

        return employee;
    }

    public override async Task<int> Delete(int id)
    {
        await SetUpDb();

        return await base.Delete(id);
    }

    public async Task<Employee> GetByCPF(string cpf)
    {
        await SetUpDb();

        return await _connection.Table<Employee>().FirstOrDefaultAsync(x => x.CPF == cpf);
    }
}


