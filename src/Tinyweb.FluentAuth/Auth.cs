using System;
using System.Collections.Generic;
using System.Linq;
using tinyweb.framework;
using System.Security.Principal;
using System.Web.Routing;

// Reuse the Tinyweb namespace 
namespace Tinyweb.FluentAuth
{
    // Auth
    // ====
    //
    // This is the primary class within the FluentAuth architecture.  It 
    // provides a single entry point for both configuration of auth and
    // validating of requests.
    public class Auth
    {
        // Internal store for all secutiry rules associated with a partiuclar 
        // handler type
        static Dictionary<Type, ICollection<Configuration>> _config = new Dictionary<Type, ICollection<Configuration>>();

        // Private constructor
        private Auth()
        {
            // Re-initialise the `_config` object every time a new instance is 
            // constructed
            _config = new Dictionary<Type, ICollection<Configuration>>();

            // set up the default handlers for `OnAccessDenied`,
            // `IsAuthenticated` and `IsInRole`
            OnAccessDenied = (c, d) => Result.None();
            IsAuthenticated = rc => rc.HttpContext.User.Identity.IsAuthenticated;
            IsInRole = (r, rc) => rc.HttpContext.User.IsInRole(r);
        }

        // For&lt;THandler>
        // ---------------
        // Instance method use to begin specification of a configuration for 
        // a handler
        public Configuration For<THandler>() where THandler : class
        {
            // Call overloaded `For<>`with the default condition that returns 
            // true
            return For<THandler>((r,d,p) => true);
        }

        // For&lt;THandler>
        // ---------------
        // Allows you to specify a rule that must match a certain condition
        public Configuration For<THandler>(Func<RequestContext, HandlerData, IPrincipal, bool> condition) where THandler : class
        {
            // create a new default configuration and associate it with a
            // handler.
            Type handler = typeof(THandler);
            Configuration configuration = new Configuration(condition);

            // create an empty configuration collection if this is the first time
            // anything has been configured for this handler
            if (!_config.ContainsKey(handler))
            {
                _config.Add(handler, new List<Configuration>());
            }

            _config[handler].Add(configuration);
            return configuration;
        }

        // Auth.Configuration
        // ======================
        //
        // Class that can be used to specify the a single configuration for a 
        // handler
        public class Configuration
        {
            internal List<string> DeniedRoles;
            internal List<string> RequiredRoles;
            internal bool IsDenyAnonymousAccess;
            internal List<Func<RequestContext, HandlerData, IPrincipal, bool>> CustomRules;
            internal AllowedVerbs AllowedVerbs;
            internal Func<RequestContext, HandlerData, IPrincipal, bool> Condition;

            // Default constructor that accepts a condition for applicability 
            // of this configuration
            public Configuration(Func<RequestContext, HandlerData, IPrincipal, bool> condition)
            {
                // Set defaults for all configuration settings
                Condition = condition;
                DeniedRoles = new List<string>();
                RequiredRoles = new List<string>();
                IsDenyAnonymousAccess = false;
                CustomRules = new List<Func<RequestContext, HandlerData, IPrincipal, bool>>();
                AllowedVerbs = AllowedVerbs.GET
                    | AllowedVerbs.POST
                    | AllowedVerbs.PUT
                    | AllowedVerbs.DELETE;                    
            }

            // DenyRoles
            // ---------
            //
            // Allows you to specify any roles that should be denied by this 
            // handler
            public Configuration DenyRoles(params string[] roles)
            {
                // When we add roles into the mix we assume that anonymous 
                // acces is also denied
                DenyAnonymousAccess();
                DeniedRoles.AddRange(roles);
                return this;
            }

            // RequiredRoles
            // -------------
            //
            // Allows you to specify any roles that should be allowed by this 
            // handler
            public Configuration RequireRoles(params string[] roles)
            {
                // When we add roles into the mix we assume that anonymous 
                // acces is also denied
                DenyAnonymousAccess();
                RequiredRoles.AddRange(roles);
                return this;
            }

            // DenyAnonymousAccess
            // -------------------
            //
            // Specify that only authenticated users may use this handler
            public Configuration DenyAnonymousAccess()
            {
                IsDenyAnonymousAccess = true;
                return this;
            }

            // AllowAnonymousAccess
            // -------------------
            //
            // Specify that non-authenticated users may use this handler
            public Configuration AllowAnonymousAccess()
            {
                IsDenyAnonymousAccess = false;
                return this;
            }

            // WithCustomRule
            // --------------
            //
            // Allows you specify a custom rule that can be used to test the 
            // applicability of a handler
            public Configuration WithCustomRule(
                Func<RequestContext, HandlerData, IPrincipal, bool> rule)
            {
                CustomRules.Add(rule);
                return this;
            }

            // AllowVerbs
            // ----------
            // 
            // Allows developer to specify the HTTP verbs that can be used 
            // against this handler
            public Configuration AllowVerbs(AllowedVerbs verbs)
            {
                AllowedVerbs = verbs;
                return this;
            }

            // And
            // ---
            //
            // Simple property that returns this instance of the configuration 
            // object.  This is an optional property that can be used to create
            // a more natural fluent syntax.  e.g.
            //
            //     c.For<RootHandler>().DenyAnonymousAccess().RequireRoles("Admin");
            //
            // can be expressed as,
            //    
            //     c.For<RootHandler>().DenyAnonymousAccess().And.RequireRoles("Admin");
            public Configuration And
            {
                get { return this; }
            }
        }

        // Test
        // ----
        //
        // Performs all applicable tests on the current handler request
        public static bool Test(RequestContext context, HandlerData data)
        {
            ICollection<Configuration> configs;
            bool hasConfigs = _config.TryGetValue(data.Type, out configs);
            bool hasApplicableConfig = false;
            bool configFailed = false;

            // If configurations exist for this handler
            if (hasConfigs)
            {
                // Grab some useful request information (verb and principal)
                AllowedVerbs verb = (AllowedVerbs)Enum.Parse(
                    typeof(AllowedVerbs),
                    context.HttpContext.Request.RequestType);
                IPrincipal principal = context.HttpContext.User;

                // iterate over each configuration for this handler and test 
                // each sequentially
                foreach (Configuration config in configs)
                {
                    // Test to see of this configuration is applicable at this 
                    // time
                    if (config.Condition(context, data, principal))
                    {
                        // Mark cycle as having an applicable configuration
                        hasApplicableConfig = true;

                        // Check if the verb is allowed
                        bool acceptableVerb = (verb & config.AllowedVerbs) != 0;
                        if (acceptableVerb)
                        {
                            // If the user is authenticated
                            if (IsAuthenticated(context))
                            {
                                // Configuration must pass 3 tests,
                                //
                                // 1. Ensure that all Required Roles (if any) are satisifed                                
                                // 2. Ensure that all Denied Roles (if any) are not present
                                // 3. Ensure that all custom rules (if any) are satisifed
                                configFailed = config.RequiredRoles.Any(role => !IsInRole(role, context))
                                    || config.DeniedRoles.Any(role => IsInRole(role, context))                                
                                    || config.CustomRules.Any(rule => !rule(context, data, principal));
                            }
                            // If the user is anonymous and we allow anonymous access
                            else if (!config.IsDenyAnonymousAccess)
                            {
                                // Ensure that all custom rules (if any) are satisifed
                                configFailed = config.CustomRules.Any(rule => !rule(context, data, principal));
                            }
                            else
                            {
                                // If we have gotten this far then the config 
                                // has failed due to anonymous access being denied
                                configFailed = true;
                            }
                        }
                        else
                        {
                            // if we have gotten this far then the config has 
                            // failed due to an unaaceptable verb
                            configFailed = true;
                        }

                        // if the config has failed the test we short circuit 
                        // and return immediatley
                        if (configFailed)
                        {
                            break;
                        }
                    }
                }
            }

            // We can return true if,
            //
            // - there or no configs for the handler
            // - if there are no APPLICABLE configs for the handler, or,
            // - if all request-applicable configs have passed for this handler
            return !hasConfigs            
                || !hasApplicableConfig            
                || (hasApplicableConfig && !configFailed);
        }

        // Validate
        // --------
        //
        // Test the request and return the default None respnose or call the 
        // OnAccessDenied Handler depending on result
        public static IResult Validate(RequestContext context, HandlerData data)
        {
            return Test(context, data) ? Result.None() : OnAccessDenied(context, data);
        }

        // Configure
        // ---------
        //
        // Allows for configuration of FluentAuth
        public static void Configure(Action<Auth> configurer)
        {
            // Create a new instance of the class.  This will effectively reset 
            // all the auth and OnAccessDenied settings each time so order 
            // is important in configuration.  Perhaps come back to this later.
            Auth config = new Auth();

            // execute developer described configuration of new instance.
            configurer(config);
        }

        // OnAccessDenied
        // --------------
        //
        // Allows developer to set a global handler for `OnAccessDenied` which 
        // will be called when the request fails to validate against a rule
        public static Func<RequestContext, HandlerData, IResult> OnAccessDenied { get; set; }

        // IsAuthenticated
        // ---------------
        //
        // Allows the developer to set a custom method for checking if a user is
        // authenticated.  This is useful if the authentication scheme isn't 
        // "normal".
        public static Func<RequestContext, bool> IsAuthenticated { get; set; }

        // IsInRole
        // --------
        //
        // Allows the developer to set a custom method for checking if a user is
        // authenticated.  This is useful if the authentication scheme isn't 
        // "normal".
        public static Func<string, RequestContext, bool> IsInRole { get; set; }
    }
}