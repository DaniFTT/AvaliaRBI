using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;
public class AssessmentCollectionService : BaseService<AssessmentCollection>
{
    private AssessmentCollectionRepository _repository;
    public AssessmentCollectionService(IBaseRepository<AssessmentCollection> repository, EmailService emailService) : base(repository, emailService)
    {
        _repository = new AssessmentCollectionRepository();
    }

    public async Task<AssessmentCollection> GetByName(string name)
    {
        return await _repository.GetByName(name);
    }
}
