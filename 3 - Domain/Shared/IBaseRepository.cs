namespace AvaliaRBI._3___Domain.Abstractions
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task ResetDb();
        Task<T> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task<int> Insert(T value);
        Task<int> Update(int id, T value);
        Task<int> Delete(int id);
    }

}
