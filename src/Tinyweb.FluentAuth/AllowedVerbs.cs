namespace Tinyweb.FluentAuth
{
    // Convenience `enum` representing the list of applicable HTTP Verbs that 
    // can be restricted via FluentAuth
    public enum AllowedVerbs
    {
        GET = 1,
        POST = 2,
        PUT = 4,
        DELETE = 8
    }
}
