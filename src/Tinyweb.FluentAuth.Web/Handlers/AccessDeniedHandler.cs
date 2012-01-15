using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tinyweb.framework;
using System.Web.Routing;
using ServiceStack.Text;

namespace Tinyweb.FluentAuth.Web.Handlers
{
    public class AccessDeniedHandler
    {
        public IResult Get(RequestContext ctx, HandlerData data)
        {
            return Result.String("Access Denied");
        }
    }
}