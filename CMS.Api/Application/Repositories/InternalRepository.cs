using CMS.Api.Application.Common;
using CMS.Api.Domain.Entities;
using MongoDB.Driver;

namespace CMS.Api.Application.Repositories;

public interface IInternalRepository
{
    Task<Company> CreateCompanyAsync(Company company, CancellationToken token = default);
    Task<List<Company>> GetCompaniesAsync(CancellationToken token = default);
    Task<Company?> GetCompanyByIdAsync(string companyId, CancellationToken token = default);
}

public class InternalRepository(IMongoClient client) : IInternalRepository
{
    public async Task<Company> CreateCompanyAsync(Company company, CancellationToken token = default)
    {
        var collection = GetInternalCollectionCompany();
        await collection.InsertOneAsync(company);
        return company;
    }

    public async Task<List<Company>> GetCompaniesAsync(CancellationToken token = default)
    {
        var collection = GetInternalCollectionCompany();
        var result = await collection.FindAsync(_ => true, cancellationToken: token);
        return await result.ToListAsync(token);
    }

    public async Task<Company?> GetCompanyByIdAsync(string companyId, CancellationToken token = default)
    {
        var collection = GetInternalCollectionCompany();
        var filter = Builders<Company>.Filter.Eq("Id", companyId);
        var result = await collection.FindAsync(filter, cancellationToken: token);
        return await result.FirstOrDefaultAsync(token);
    }


    private IMongoCollection<Company> GetInternalCollectionCompany() =>
        client.GetDatabase(Constants.InternalDb.Database_Name)
              .GetCollection<Company>(Constants.InternalDb.CollectionNames.Companies);
}
