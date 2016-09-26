namespace HTTPStatusBot.Modules
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Nancy;
    using Nancy.ModelBinding;
    using Dialogs;

    public class HomeModule : NancyModule
    {
        public static string MicrosoftAppId { get; set; }
        public static string MicrosoftAppIdSettingName { get; set; }
        public static bool DisableSelfIssuedTokens { get; set; }
        public static string OpenIdConfigurationUrl { get; set; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;

        public HomeModule()
        {
            // this.RequiresBotAuthentication();

            Get("/", args => "Hi");

            Post("/", async _ =>
            {

                var valid = await this.BotAuthenticationValid();
                if (!valid)
                {
                    return new Response
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Headers =
                             new Dictionary<string, string>()
                             {
                                {"WWW-Authenticate", $"Bearer realm=\"{this.Context.Request.Url.HostName}\""}
                             }
                    };
                }

                var activity = this.Bind<Activity>();

                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => new RootDialog());
                        break;
                }

                return HttpStatusCode.Accepted;
            });
        }

        private async Task<bool> BotAuthenticationValid()
        {
            MicrosoftAppId = MicrosoftAppId ??
                ConfigurationManager.AppSettings[MicrosoftAppIdSettingName ?? "MicrosoftAppId"];

            if (Debugger.IsAttached && string.IsNullOrEmpty(MicrosoftAppId))
            {
                // then auth is disabled
                return true;
            }

            var tokenExtractor =
                new JwtTokenExtractor(JwtConfig.GetToBotFromChannelTokenValidationParameters(MicrosoftAppId),
                    OpenIdConfigurationUrl);

            var identity = await tokenExtractor.GetIdentityAsync(this.Context.Request.Headers.Authorization);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !DisableSelfIssuedTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromMSATokenValidationParameters,
                    JwtConfig.ToBotFromMSAOpenIdMetadataUrl);

                identity = await tokenExtractor.GetIdentityAsync(this.Context.Request.Headers.Authorization);

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
                return true;
            }

            var activity = this.Bind<Activity>();

            if (!string.IsNullOrWhiteSpace(activity.ServiceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            }
            else
            {
                Trace.TraceWarning("No activity in the Bot Authentication Action Arguments");
            }

            this.Context.CurrentUser = new ClaimsPrincipal(identity);
            return false;
        }
    }
}
