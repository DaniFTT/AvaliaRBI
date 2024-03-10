using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;

public class SectorService : BaseService<Sector>
{
    private SectorRepository _repository;
    public SectorService(IBaseRepository<Sector> repository, EmailService emailService) : base(repository, emailService)
    {
        _repository = new SectorRepository();
    }

    public async Task<Sector> GetByName(string name)
    {
        return await _repository.GetByName(name);
    }
}
