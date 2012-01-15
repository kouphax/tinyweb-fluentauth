using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tinyweb.framework;

namespace Tinyweb.FluentAuth.Web.Handlers
{
    public class SecretHandler
    {
        public IResult Get()
        {
            return Result.String("Secret Area - Access Granted");
        }
    }
}