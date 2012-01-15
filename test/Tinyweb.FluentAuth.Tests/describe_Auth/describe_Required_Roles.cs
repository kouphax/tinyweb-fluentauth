using FakeItEasy;
using NSpec;
using Tinyweb.FluentAuth.Tests.Support;

namespace Tinyweb.FluentAuth.Tests
{
    class describe_Required_Roles : describe_Auth
    {
        void before_each()
        {            
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);
        }
        
        void given_user_doesnt_have_a_required_role()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().RequireRoles("Role4"));

            act = PerformAuthTest;

            it["should fail the auth test"] = () =>
                permitted.should_be_false();

            context["and the user is anonymous but anonymous access is allowed"] = () =>
            {
                before = () =>
                    A.CallTo(() => user.Identity.IsAuthenticated)
                        .Returns(false);

                act = PerformAuthTest;

                it["should fail the auth test"] = () =>
                    permitted.should_be_false();
            };
        }

        void given_user_has_a_required_role()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().RequireRoles("Role3"));

            act = PerformAuthTest;

            it["should pass the auth test"] = () =>
                permitted.should_be_true();
        }

        void given_user_has_multiple_required_roles()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().RequireRoles("Role1", "Role2"));

            act = PerformAuthTest;

            it["should pass the auth test"] = () =>
                permitted.should_be_true();
        }

        void given_user_has_only_some_of_the_required_roles()
        {
            before = () =>
                Auth.Configure(c =>
                    c.For<FakeHandler>().RequireRoles("Role2", "Role5"));

            act = PerformAuthTest;

            it["should still fail the auth test"] = () =>
                permitted.should_be_false();
        }
    }
}
