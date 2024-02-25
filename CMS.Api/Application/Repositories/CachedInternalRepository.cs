using CMS.Api.Application.Services;
using CMS.Api.Domain.Entities;

namespace CMS.Api.Application.Repositories;

public class CachedInternalRepository(ICacheService cacheService, IInternalRepository repository) : IInternalRepository
{
    private const string keyPrefix = "company-";
    public async Task<Company> CreateCompanyAsync(Company company, CancellationToken token = default)
    {
        var result = await repository.CreateCompanyAsync(company, token);

        await cacheService.SetAsync($"{keyPrefix}{company.Id}", result, token);
        return result;
    }

    public async Task<List<Company>> GetCompaniesAsync(CancellationToken token = default)
    {
        var companies = await repository.GetCompaniesAsync(token);
        var t = companies.Select(x => cacheService.SetAsync($"{keyPrefix}{x.Id}", x, token));
        await Task.WhenAll(t);

        return companies;
    }

    public async Task<Company?> GetCompanyByIdAsync(string companyId, CancellationToken token = default)
    {
        return await cacheService.GetAsync($"{keyPrefix}{companyId}", () => repository.GetCompanyByIdAsync(companyId, token), token);
    }
}
