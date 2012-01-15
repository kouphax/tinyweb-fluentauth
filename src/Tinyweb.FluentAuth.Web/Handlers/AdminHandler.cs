using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tinyweb.framework;

namespace Tinyweb.FluentAuth.Web.Handlers
{
    public class AdminHandler
    {
        public IResult Get()
        {
            return Result.String("Admin Area - Access Granted");
        }
    }
}