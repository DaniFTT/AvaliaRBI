using AvaliaRBI._3___Domain;
using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._4___Repository;

public class DepartmentRepository : BaseRepository<Department>
{
    private AssessmentCollectionRepository _collectionRepository;
    private SectorRepository _sectorRepository;
    public DepartmentRepository() : base()
    {
        _collectionRepository = new AssessmentCollectionRepository();
        _sectorRepository = new SectorRepository();
    }

    public override async Task<IEnumerable<Department>> GetAll()
    {
        await SetUpDb();

        var departments = (await base.GetAll()).ToArray();
        if (!departments.Any())
            return Array.Empty<Department>();

        var sectors = (await _sectorRepository.GetAll()).Where(d => departments.Select(p => p.SectorId).Contains(d.Id)).ToArray();

        foreach (var department in departments)
            department.Sector = sectors.FirstOrDefault(d => d.Id == department.SectorId);

        return departments;
    }

    public override async Task<Department> GetById(int id)
    {
        await SetUpDb();

        var department = await base.GetById(id);
        if (department != null)
            department.Sector = await _sectorRepository.GetById(department.SectorId.Value);

        return department;
    }

    public override async Task<int> Delete(int id)
    {
        await SetUpDb();
        var value = await GetById(id);
        if (value == null)
            return 0;

        var collections = (await _collectionRepository.GetAll()).Where(c => c.Departments.Any(d => d.Id == id)).ToArray();
        if (collections.Any())
            return -1;

        var positions = await _connection.Table<PositionJob>().Where(c => c.DepartmentId == id).ToListAsync();
        if (positions.Any())
            return -1;

        return await _connection.DeleteAsync(value);
    }

    public async Task<Department> GetByName(string name)
    {
        await SetUpDb();
        name = name.NormalizeString();
        return await _connection.Table<Department>().FirstOrDefaultAsync(x => x.Name == name);
    }
}

