using shortid.Configuration;
using shortid;

namespace CMS.Api.Application.Common;

public static class Utils
{
    public static string GenerateId() => ShortId.Generate(new GenerationOptions(true, false, 12)).ToLowerInvariant();
}
