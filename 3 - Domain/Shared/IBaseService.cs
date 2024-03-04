using Ardalis.Result;

namespace AvaliaRBI._3___Domain.Abstractions
{
    public interface IBaseService<T> where T : BaseEntity
    {
        Task ResetDb();
        Task<Result<T>> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task<Result<T>> Insert(T value);
        Task<Result<T>> Update(int id, T value);
        Task<Result> Delete(int id);
    }
}
