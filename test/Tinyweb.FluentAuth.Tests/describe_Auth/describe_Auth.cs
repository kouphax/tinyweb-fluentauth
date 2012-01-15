using System.Linq;
using System.Security.Principal;
using System.Web.Routing;
using FakeItEasy;
using NSpec;
using Tinyweb.FluentAuth.Tests.Support;
using tinyweb.framework;

namespace Tinyweb.FluentAuth.Tests
{
    class describe_Auth : nspec
    {
        protected bool permitted = false;
        protected HandlerData data;
        protected RequestContext rc;
        protected IPrincipal user;

        protected void PerformAuthTest()
        {
            // performs basic secutiry test
            permitted = Auth.Test(rc, data);
        }

        void before_each()
        {
            // create the fakes
            data = new HandlerData() { Type = typeof(FakeHandler) };
            rc = A.Fake<RequestContext>();
            user = A.Fake<IPrincipal>();

            // setup default calls
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(false);
            A.CallTo(() => user.IsInRole(""))
                .WithAnyArguments() 
                .ReturnsLazily(o => new [] { "Role1", "Role2", "Role3" }.Contains(o.Arguments[0]));
            A.CallTo(() => rc.HttpContext.Request.RequestType).Returns("GET");
            A.CallTo(() => rc.HttpContext.User).Returns(user);            
        }

        void given_no_configuration_exists_for_a_handler()
        {
            act = PerformAuthTest;

            it["should pass the auth test"] = () => 
                permitted.should_be_true();
        }
    }
}
