using AvaliaRBI._3___Domain;
using AvaliaRBI.Shared.Extensions;

namespace AvaliaRBI._4___Repository;

public class SectorRepository : BaseRepository<Sector>
{
    public SectorRepository() : base()
    {

    }

    public override async Task<int> Delete(int id)
    {
        await SetUpDb();
        var value = await GetById(id);
        if (value == null)
            return 0;

        var departments = await _connection.Table<Department>().Where(c => c.SectorId == id).ToListAsync();
        if (departments.Any())
            return -1;

        return await _connection.DeleteAsync(value);
    }

    public async Task<Sector> GetByName(string name)
    {
        await SetUpDb();
        name = name.NormalizeString();
        return await _connection.Table<Sector>().FirstOrDefaultAsync(x => x.Name == name);
    }
}

