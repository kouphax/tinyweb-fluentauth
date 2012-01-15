using FakeItEasy;
using NSpec;
using Tinyweb.FluentAuth.Tests.Support;

namespace Tinyweb.FluentAuth.Tests
{
    class describe_Deny_Roles : describe_Auth
    {
        void before_each()
        {            
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);
        }

        void given_the_user_has_no_restricted_roles()
        {
            before = () =>
            {
                A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);
                Auth.Configure(c => c.For<FakeHandler>().DenyRoles("Role4", "Role5"));
            };

            act = () =>
            {
                PerformAuthTest();
            };

            it["should pass the auth test"] = () =>
                permitted.should_be_true();
        }

        void given_the_user_has_1_restricted_role()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().DenyRoles("Role3"));

            act = () => { PerformAuthTest(); };

            it["should fail the auth test"] = () =>
                permitted.should_be_false();
        }

        void given_the_user_has_multiple_restricted_roles()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().DenyRoles("Role1", "Role3"));

            act = PerformAuthTest;

            it["should fail the auth test"] = () =>
                permitted.should_be_false();
        }
    }
}
