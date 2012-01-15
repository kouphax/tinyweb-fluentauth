using NSpec;
using Tinyweb.FluentAuth.Tests.Support;
using tinyweb.framework;

namespace Tinyweb.FluentAuth.Tests
{
    class describe_Validate : describe_Auth
    {
        IResult result;

        void given_a_failed_call_to_Validate_without_configuration()
        {
            before = () => Auth.Configure(c => c.For<FakeHandler>().DenyAnonymousAccess());
            act = () => result = Auth.Validate(rc, data);
            it["should return an empty result"] = () =>
                result.GetType().should_be_same(typeof(NoneResult));
        }

        void given_a_failed_call_to_Validate_with_configuration()
        {
            before = () =>
            {                
                Auth.Configure(c => c.For<FakeHandler>().DenyAnonymousAccess());
                Auth.OnAccessDenied = (c, d) => Result.String("result");
            };

            act = () => result = Auth.Validate(rc, data);
            it["should return an the access denied result"] = () =>
                (result as StringResult).Data.should_be("result");
        }
    }
}
