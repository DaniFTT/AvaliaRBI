using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._3___Domain.Enum;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;

public class AssessmentEmployeeService : BaseService<AssessmentEmployee>
{
    private AssessmentEmployeeRepository _repository;
    public AssessmentEmployeeService(IBaseRepository<AssessmentEmployee> repository, EmailService emailService) : base(repository, emailService)
    {
        _repository = new AssessmentEmployeeRepository();
    }

    public async Task<List<AssessmentEmployee>> GetAssessmentEmployeesByAssessmentId(int assessmentId, AssessmentType assessmentType, bool updatesValue = false)
    {
        return await _repository.GetAssessmentEmployeesByAssessmentId(assessmentId, assessmentType, updatesValue);
    }

    public async Task<AssessmentEmployee> GetAssessmentEmployeeByAssessmentAndEmployeeId(int assessmentId, int employeeId)
    {
        return await _repository.GetAssessmentEmployeeByAssessmentAndEmployeeId(assessmentId, employeeId);
    }

    public async Task<AssessmentEmployee[]> GetLastTreeAssessmentEmployee(DateTime referenceDate, int employeeId)
    {
        return await _repository.GetLastTreeAssessmentEmployee(referenceDate, employeeId);
    }
}

