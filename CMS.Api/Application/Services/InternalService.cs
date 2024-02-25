using CMS.Api.Application.Repositories;
using CMS.Api.Domain.Entities;

namespace CMS.Api.Application.Services;

public interface IInternalService
{
    Task<Company> RegisterCompanyAsync(string companyName, CancellationToken token = default);
    Task<List<Company>> GetCompaniesAsync(CancellationToken token = default);
    Task<Company?> GetCompanyByIdAsync(string companyId, CancellationToken token = default);
}

public class InternalService(IInternalRepository repository) : IInternalService
{
    public async Task<Company> RegisterCompanyAsync(string companyName, CancellationToken token = default)
    {
        Company company = Company.Create(companyName);
        return await repository.CreateCompanyAsync(company, token);
    }

    public Task<List<Company>> GetCompaniesAsync(CancellationToken token = default)
    {
        return repository.GetCompaniesAsync(token);
    }

    public Task<Company?> GetCompanyByIdAsync(string companyId, CancellationToken token = default)
    {
        return repository.GetCompanyByIdAsync(companyId, token);
    }

}
