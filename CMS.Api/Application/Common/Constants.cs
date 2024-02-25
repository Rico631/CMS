namespace CMS.Api.Application.Common;

public static class Constants
{
    public static class InternalDb
    {
        public const string Database_Name = "_internal";
        public static class CollectionNames
        {
            public const string Companies = "companies";
        }
        
    }

    public static class CompanyDb
    {
        public static class CollectionNames
        {
            public const string Messages = "messages";
            public const string PublishedMessages = "messages-published";
        }
        
    }
    
}
