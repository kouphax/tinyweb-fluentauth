using FakeItEasy;
using NSpec;
using Tinyweb.FluentAuth.Tests.Support;

namespace Tinyweb.FluentAuth.Tests
{
    class describe_ConditionalRules : describe_Auth
    {
        void given_multiple_rules_where_only_1_applies_but_is_valid()
        {
            before = () =>
            {
                A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);
                Auth.Configure(c =>
                {
                    c.For<FakeHandler>().DenyAnonymousAccess();
                    c.For<FakeHandler>((r, d, p) => p.Identity.IsAuthenticated)
                        .WithCustomRule((r, d, p) => true);
                });
            };

            act = PerformAuthTest;

            it["should pass the secutiry test"] = () =>
                permitted.should_be_true();
        }

        void given_multiple_rules_where_only_1_applies_but_is_invalid()
        {
            before = () =>
            {
                A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);
                Auth.Configure(c =>
                {
                    c.For<FakeHandler>().DenyAnonymousAccess();
                    c.For<FakeHandler>((r, d, p) => p.Identity.IsAuthenticated)
                        .WithCustomRule((r, d, p) => false);
                    c.For<FakeHandler>((r, d, p) => p.Identity.IsAuthenticated)
                        .WithCustomRule((r, d, p) => true);
                });
            };

            act = PerformAuthTest;

            it["should fail the secutiry test"] = () =>
                permitted.should_be_false();
        }
    }
}
