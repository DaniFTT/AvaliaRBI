using Ardalis.Result;
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

    public async Task<Result<MonthlyAssessment>> GetByIdUpdated(int id)
    {
        try
        {
            return Result<MonthlyAssessment>.Success(await _repository.GetByIdUpdated(id));
        }
        catch (Exception e)
        {
            return Result<MonthlyAssessment>.Error("Erro obter a Avaliação Mensal");
        }
    }
}