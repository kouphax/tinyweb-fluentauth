using NSpec;
using Tinyweb.FluentAuth.Tests.Support;

namespace Tinyweb.FluentAuth.Tests
{
    class given_the_current_verb_is_disallowed : describe_Auth
    {
        void when_allowed_verbs_is_set_to_a_single_verb()
        {
            before = () =>
                Auth.Configure(c => c.For<FakeHandler>()
                    .AllowVerbs(AllowedVerbs.DELETE));

            act = PerformAuthTest;

            it["should fail the auth test"] = () =>
                permitted.should_be_false();
        }

        void when_allowed_verbs_is_set_to_multiple_verbs()
        {
            before = () =>
                Auth.Configure(c => c.For<FakeHandler>()
                    .AllowVerbs(AllowedVerbs.DELETE
                        | AllowedVerbs.POST
                        | AllowedVerbs.PUT));

            act = PerformAuthTest;

            it["should fail the auth test"] = () =>
                permitted.should_be_false();
        }
    }
}
