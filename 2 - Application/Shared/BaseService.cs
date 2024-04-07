using Ardalis.Result;
using AvaliaRBI._2___Application.Shared;

namespace AvaliaRBI._3___Domain.Abstractions
{
    public abstract class BaseService<T> : IBaseService<T> where T : BaseEntity, new()
    {
        private IBaseRepository<T> _repository;
        protected NotificationsService _notificationsService;
        public BaseService(IBaseRepository<T> repository, NotificationsService notificationService)
        {
            _repository = repository;
            _notificationsService = notificationService;
        }

        public virtual async Task<Result<T>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetById(id);

                if (result != null)
                    return Result<T>.Success(result);

                return Result<T>.NotFound("Registro não encontrado!");
            }
            catch (Exception e)
            {
                return Result<T>.Error("Não foi possível obter esse registro!");
            }
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            try
            {
                return await _repository.GetAll();
            }
            catch (Exception e)
            {
                return Array.Empty<T>();
            }
        }

        public virtual async Task<Result<T>> Insert(T value)
        {
            try
            {
                var result = await _repository.Insert(value);

                if (result == 1)
                    return Result<T>.Success(value);

                return Result<T>.Error("Não foi possível criar esse registro!");
            }
            catch (Exception e)
            {
                return Result<T>.Error("Não foi possível criar esse registro!");
            }
        }

        public virtual async Task<Result<T>> Update(int id, T value)
        {
            try
            {
                var result = await _repository.Update(id, value);

                if (result == 1)
                    return Result<T>.Success(value);

                return Result<T>.Error("Não foi possível atualizar esse registro!");
            }
            catch (Exception e)
            {
                return Result<T>.Error("Não foi possível atualizar esse registro!");
            }
        }

        public virtual async Task<Result> Delete(int id)
        {
            try
            {
                var result = await _repository.Delete(id);

                if (result == 1)
                    return Result.Success();
                else if (result == -1)
                    return Result.Error("Não é possível deletar esse registro, poís ele possui dependências!");

                return Result.Error("Não foi possível deletar esse registro!");
            }
            catch (Exception e)
            {
                return Result.Error("Não foi possível deletar esse registro!");
            }
        }

        public async Task ResetDb()
        {
            await _repository.ResetDb();
        }
    }
}
