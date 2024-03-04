using AvaliaRBI._3___Domain;
using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._4___Repository;

public class PositionRepository : BaseRepository<PositionJob>
{
    private DepartmentRepository _departmentRepository;
    public PositionRepository() : base()
    {
        _departmentRepository = new DepartmentRepository();
    }

    public override async Task<IEnumerable<PositionJob>> GetAll()
    {
        await SetUpDb();

        var positions = (await base.GetAll()).ToArray();
        if (!positions.Any())
            return Array.Empty<PositionJob>();

        var departments = (await _departmentRepository.GetAll()).Where(d => positions.Select(p => p.DepartmentId).Contains(d.Id)).ToArray();

        foreach (var position in positions)
            position.Department = departments.FirstOrDefault(d => d.Id == position.DepartmentId);

        return positions;
    }

    public override async Task<PositionJob> GetById(int id)
    {
        await SetUpDb();

        var position = await base.GetById(id);
        if (position != null)
            position.Department = await _departmentRepository.GetById(position.DepartmentId.Value);

        return position;
    }

    public override async Task<int> Delete(int id)
    {
        var employees = await _connection.Table<Employee>().Where(e => e.PositionId == id).ToListAsync();
        if (employees.Any())
            return -1;

        return await base.Delete(id);
    }

    public async Task<PositionJob> GetByName(string name)
    {
        await SetUpDb();
        name = name.NormalizeString();
        return await _connection.Table<PositionJob>().FirstOrDefaultAsync(x => x.Name == name);
    }
}

