using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;

public class MonthlyAssessmentService : BaseService<MonthlyAssessment>
{
    private MonthlyAssessmentRepository _repository;
    public MonthlyAssessmentService(IBaseRepository<MonthlyAssessment> repository, EmailService emailService) : base(repository, emailService)
    {
        _repository = new MonthlyAssessmentRepository();
    }

    public async Task<IEnumerable<MonthlyAssessment>> GetByReferenceDate(DateTime referenceDate)
    {
        return await _repository.GetByReferenceDate(referenceDate);
    }

    public async Task<MonthlyAssessment> GetByIdUpdated(int id)
    {
        return await _repository.GetByIdUpdated(id);
    }
}