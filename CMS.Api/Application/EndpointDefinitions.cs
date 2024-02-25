using CMS.Api.Application.Dtos;
using CMS.Api.Application.Repositories;
using CMS.Api.Application.Services;
using CMS.Api.Domain.Entities;
using CMS.Api.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Application;

public static class EndpointDefinitions
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("api");

        group.InternalEndpoints();

        group.NewsEndpoints();

        group.FileEndpoints();


        return app;
    }

    public static RouteGroupBuilder InternalEndpoints(this RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("internal").WithTags("Internal");

        group.MapGet("/company",
            async (CancellationToken token, IInternalService service) =>
            {
                var result = await service.GetCompaniesAsync(token);

                return Results.Ok(result);
            })
            .WithName("GetCompanies")
            .Produces<List<Company>>();

        group.MapGet("/company/{companyId}",
            async (CancellationToken token, IInternalService service, string companyId) =>
            {
                var result = await service.GetCompanyByIdAsync(companyId, token);

                return Results.Ok(result);
            })
            .WithName("GetCompanyById")
            .Produces<Company>();

        group.MapPost("/company",
            async (CancellationToken token, IInternalService service, CreateCompanyDto company) =>
            {
                var result = await service.RegisterCompanyAsync(company.Name, token);

                return Results.Ok(result);
            })
            .WithName("RegisterCompany")
            .Produces<Company>();


        return group;
    }

    public static RouteGroupBuilder NewsEndpoints(this RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("message").WithTags("Message");
        group.MapGet("/{companyId}",
            async (CancellationToken token, IMessageService service, string companyId) =>
            {
                var result = await service.GetTypedAsync<NewsMessage>(companyId, token);

                if (result.IsError)
                    return Results.Problem(result.FirstError.Description, result.FirstError.Code);

                return Results.Ok(result.Value?.Select(MessageDto.Create) ?? []);
            })
            .WithName("GetMessages")
            .Produces<MessageDto>();

        group.MapPost("/{companyId}",
            async (CancellationToken token, IMessageService service, [FromRoute] string companyId, [FromBody] CreateMessageDto dto) =>
            {
                var result = dto.Type switch
                {
                    MessageTypes.News => await service.CreateDefaultAsync<NewsMessage>(companyId, token),
                    MessageTypes.Telegram => DomainErrors.Common.NotImplement(dto.Type.ToString()),
                    _ => DomainErrors.Common.NotImplement(dto.Type.ToString())
                };

                if (result.IsError)
                    return Results.Problem(result.FirstError.Description, result.FirstError.Code);

                return Results.Ok(MessageDto.Create(result.Value));
            })
            .WithName("CreateMessage")
            .Produces<MessageDto>();

        group.MapPost("/{companyId}/{messageId}",
            async (CancellationToken token, IMessageService service, string companyId, string messageId, [FromForm] IFormFile file) =>
            {
                var result = await service.AddAttachmentAsync(companyId, messageId, file.FileName, file.ContentType, file.Length, file.OpenReadStream(), token);

                if (result.IsError)
                    return Results.Problem(result.FirstError.Description, result.FirstError.Code);

                return Results.Ok(MessageAttachmentDto.Create(result.Value));
            })
            .WithName("AddMessageAttachment")
            .DisableAntiforgery()
            .Produces<MessageAttachmentDto>();

        return group;
    }


    public static RouteGroupBuilder FileEndpoints(this RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("file").WithTags("File");
        group.MapGet("/attachment/{companyId}/{messageId}/{attachmentId}",
            async (CancellationToken token, IInternalService internalService, IStorageRepository storageRepository, string companyId, string messageId, string attachmentId) =>
            {

                var company = await internalService.GetCompanyByIdAsync(companyId, token);
                if (company is null)
                    return Results.BadRequest();

                var result = await storageRepository.GetFileAsync(company.Storage.BucketName, $"{messageId}/{attachmentId}", token);

                if (result is null)
                    return Results.NotFound();

                return Results.File(result.Stream, contentType: result.ContentType, fileDownloadName: result.FileName);
            })
            .WithName("GetAttachment");

        return group;
    }
}
