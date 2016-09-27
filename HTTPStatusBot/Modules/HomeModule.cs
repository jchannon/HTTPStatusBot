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
        public HomeModule()
        {
            this.RequiresBotAuthentication();

            Get("/", args => "Hi");

            Post("/", async _ =>
            {
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
    }
}
