namespace HTTPStatusBot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Nancy;
    using Nancy.Extensions;
    using Nancy.ModelBinding;

    public static class BotExtensions
    {
        public static string MicrosoftAppId { get; set; }
        public static string MicrosoftAppIdSettingName { get; set; }
        public static bool DisableSelfIssuedTokens { get; set; }
        public static string OpenIdConfigurationUrl { get; set; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;

        public static void RequiresBotAuthentication(this INancyModule module)
        {
            module.AddBeforeHookOrExecute(AuthenticateBot(module), "Requires Bot Authentication");
        }

        public static Func<NancyContext, Response> AuthenticateBot(INancyModule module)
        {
            return ctx =>
            {
                //Has to be run async as calling .Result on the GetIdentityAsync resulted in deadlock
                var response = Task.Run<Response>(async () => await GetResponse(module, ctx));
                return response.Result;
            };
        }

        private static async Task<Response> GetResponse(INancyModule module, NancyContext ctx)
        {
            MicrosoftAppId = MicrosoftAppId ??
                ConfigurationManager.AppSettings[MicrosoftAppIdSettingName ?? "MicrosoftAppId"];

            if (Debugger.IsAttached && string.IsNullOrEmpty(MicrosoftAppId))
            {
                // then auth is disabled
                return null;
            }

            var tokenExtractor =
                new JwtTokenExtractor(JwtConfig.GetToBotFromChannelTokenValidationParameters(MicrosoftAppId),
                    OpenIdConfigurationUrl);

            var identity = await tokenExtractor.GetIdentityAsync(ctx.Request.Headers.Authorization);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !DisableSelfIssuedTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromMSATokenValidationParameters,
                    JwtConfig.ToBotFromMSAOpenIdMetadataUrl);

                identity = await tokenExtractor.GetIdentityAsync(ctx.Request.Headers.Authorization);

                // Check to make sure the app ID in the token is ours
                if (identity != null)
                {
                    // If it doesn't match, throw away the identity
                    if (tokenExtractor.GetBotIdFromClaimsIdentity(identity) != MicrosoftAppId)
                    {
                        identity = null;
                    }
                }
            }

            // Still no identity? Fail out.
            if (identity == null)
            {
                //https://github.com/Microsoft/BotBuilder/blob/master/CSharp/Library/Microsoft.Bot.Connector/JwtTokenExtractor.cs#L87
                //tokenExtractor.GenerateUnauthorizedResponse(actionContext);
                return new Response
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Headers =
                        new Dictionary<string, string>()
                        {
                            { "WWW-Authenticate", $"Bearer realm=\"{ctx.Request.Url.HostName}\"" }
                        }
                };
            }

            var activity = module.Bind<Activity>();

            if (!string.IsNullOrWhiteSpace(activity.ServiceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            }
            else
            {
                Trace.TraceWarning("No activity in the Bot Authentication Action Arguments");
            }

            ctx.CurrentUser = new ClaimsPrincipal(identity);

            return null;
        }
    }
}
