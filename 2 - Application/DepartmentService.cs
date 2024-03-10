using AvaliaRBI._2___Application.Shared;
using AvaliaRBI._3___Domain;
using AvaliaRBI._3___Domain.Abstractions;
using AvaliaRBI._4___Repository;

namespace AvaliaRBI._2___Application;

public class DepartmentService : BaseService<Department>
{
    private DepartmentRepository _repository;
    public DepartmentService(IBaseRepository<Department> repository, EmailService emailService) : base(repository, emailService)
    {
        _repository = new DepartmentRepository();
    }

    public async Task<Department> GetByName(string name)
    {
        return await _repository.GetByName(name);
    }
}
