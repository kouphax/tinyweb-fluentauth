using System.Web.Routing;
using tinyweb.framework;

namespace Tinyweb.FluentAuth
{
    // Simple Tinyweb Filter that can be used to test requests against 
    // pre-configured auth
    public class AuthFilter
    {
        public IResult Before(RequestContext context, HandlerData data)
        {
            // Simple validate test but an more controllable example could be 
            // implemented like so
            // 
            //     bool granted = Auth.Test(context, data);
            //     if (!granted)
            //     {
            //         return Auth.OnAccessDenied(context, data);
            //     }
            //      
            //     return Result.None();
            return Auth.Validate(context, data);
        }
    }
}
