using NSpec;
using Tinyweb.FluentAuth.Tests.Support;

namespace Tinyweb.FluentAuth.Tests
{
    class when_user_is_unathenticated : describe_Auth
    {
        void given_unauthenticated_users_are_disallowed()
        {
            before = () =>
                Auth.Configure(c => c.For<FakeHandler>()
                    .DenyAnonymousAccess());

            act = PerformAuthTest;

            it["should fail the auth test"] = () =>
                permitted.should_be_false();
        }
    }
}
