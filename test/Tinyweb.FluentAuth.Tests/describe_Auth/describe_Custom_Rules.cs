using FakeItEasy;
using NSpec;
using Tinyweb.FluentAuth.Tests.Support;
using tinyweb.framework;

namespace Tinyweb.FluentAuth.Tests
{

    class describe_Custom_Rules : describe_Auth
    {
        void given_custom_rules_are_specified()
        {
            new[] { false, true }.ForEach(authenticated =>
            {
                before = () => A.CallTo(() => user.Identity.IsAuthenticated).Returns(authenticated);
                context[string.Format("and the user is {0} Authenticated", authenticated ? string.Empty : "Not")] = () =>
                {
                    context["when the user passes all custom rules"] = () =>
                    {
                        before = () => Auth.Configure(c => c.For<FakeHandler>()
                            .WithCustomRule((ctx, d, p) => true)
                            .WithCustomRule((ctx, d, p) => true)
                            .WithCustomRule((ctx, d, p) => true));

                        act = PerformAuthTest;

                        it["should pass the auth test"] = () =>
                            permitted.should_be_true();
                    };

                    context["when the user passes some custom rules"] = () =>
                    {

                        before = () => Auth.Configure(c => c.For<FakeHandler>()
                            .WithCustomRule((ctx, d, p) => true)
                            .WithCustomRule((ctx, d, p) => false)
                            .WithCustomRule((ctx, d, p) => true));

                        act = PerformAuthTest;

                        it["should fail the auth test"] = () =>
                            permitted.should_be_false();
                    };

                    context["when the user fails all custom rules"] = () =>
                    {
                        before = () => Auth.Configure(c => c.For<FakeHandler>()
                            .WithCustomRule((ctx, d, p) => false)
                            .WithCustomRule((ctx, d, p) => false)
                            .WithCustomRule((ctx, d, p) => false));

                        act = PerformAuthTest;

                        it["should fail the auth test"] = () =>
                            permitted.should_be_false();
                    };
                };
            });
        }
    }
}
