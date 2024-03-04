using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;

public class PositionService : BaseService<PositionJob>
{
    private PositionRepository _repository;
    public PositionService(IBaseRepository<PositionJob> repository) : base(repository)
    {
        _repository = new PositionRepository();
    }

    public async Task<PositionJob> GetByName(string name)
    {
        return await _repository.GetByName(name);
    }
}


