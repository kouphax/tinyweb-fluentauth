using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tinyweb.framework;
using Tinyweb.FluentAuth.Web.Handlers;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Tinyweb.FluentAuth.Web.Activators.TinywebActivator), "Activate")]
namespace Tinyweb.FluentAuth.Web.Activators
{
    public class TinywebActivator
    {
        public static void Activate()
        {
            Auth.OnAccessDenied = (ctx, data) =>
            {
                return Result.Redirect<AccessDeniedHandler>();
            };

            Auth.Configure(c =>
            {
                c.For<RootHandler>().DenyAnonymousAccess();
                c.For<AdminHandler>().RequireRoles("Admin");
                c.For<UserHandler>().DenyRoles("Admin");
                c.For<SecretHandler>().AllowVerbs(AllowedVerbs.GET | AllowedVerbs.POST);
            });

            tinyweb.framework.Tinyweb.Init();            
        }
    }
}