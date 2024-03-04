using AvaliaRBI._3___Domain.Abstractions;
using SQLite;

namespace AvaliaRBI._4___Repository;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
{
    protected SQLiteAsyncConnection _connection;
    public BaseRepository() { }

    protected async Task SetUpDb()
    {
        try
        {
            if (_connection == null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"AvaliaRBI.db3");

                _connection = new SQLiteAsyncConnection(dbPath);
                var result = await _connection.CreateTableAsync<T>();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public virtual async Task<T> GetById(int id)
    {
        await SetUpDb();
        return await _connection.Table<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAll()
    {
        await SetUpDb();
        return await _connection.Table<T>().ToListAsync();
    }

    public virtual async Task<int> Insert(T value)
    {
        await SetUpDb();
        return await _connection.InsertAsync(value);
    }

    public virtual async Task<int> Update(int id, T value)
    {
        await SetUpDb();
        value.Id = id;
        return await _connection.UpdateAsync(value);
    }

    public virtual async Task<int> Delete(int id)
    {
        await SetUpDb();
        var value = await GetById(id);
        if (value == null)
            return 0;

        return await _connection.DeleteAsync(value);
    }

    public async Task ResetDb()
    {
        await SetUpDb();
        await _connection.DropTableAsync<T>();
        await _connection.CreateTableAsync<T>();
    }
}
