namespace HTTPStatusBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using HTTPStatusBot.Model;
    using HTTPStatusBot.SimpleJson;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using System.Reflection;

    [Serializable]
    [LuisModel("1ea0d2b8-0db6-4cd6-b5c6-1f1923927826", "3eff2c8256484419b57890e80c18578d")]
    public class RootDialog : LuisDialog<object>
    {
        public static string GetExecutionPath()
        {
            var codeBase = typeof(RootDialog).GetTypeInfo().Assembly.CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var root = Path.GetDirectoryName(path);
            return root;
        }

        private static readonly List<StatusCodeInfo> StatusCodeList = SimpleJson.DeserializeObject<List<StatusCodeInfo>>(File.ReadAllText(Path.Combine(GetExecutionPath(),"statuscodes.json")));

        [LuisIntent("StatusCodeIntent")]
        public async Task HandleStatusCodeIntent(IDialogContext context, LuisResult result)
        {
            EntityRecommendation statuscode;
            if (!result.TryFindEntity("builtin.number", out statuscode))
            {
                statuscode = new EntityRecommendation(type: "number") { Entity = "911" };
            }

            var statuscodeInfo = StatusCodeList.FirstOrDefault(x => x.code == statuscode.Entity);
            if (statuscodeInfo == null)
            {
                await context.PostAsync("Oops I don't know that status code, sorry...");
                context.Wait(this.MessageReceived);
                return;
            }

            await context.PostAsync("I believe what you are looking for is:");
            await context.PostAsync($"Status Code : {statuscodeInfo.code} Meaning : {statuscodeInfo.meaning} Description : {statuscodeInfo.description} {statuscodeInfo.imageurl}");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("StatusCodeMeaningIntent")]
        public async Task HandleStatusDescriptionIntent(IDialogContext context, LuisResult result)
        {
            EntityRecommendation statuscodemeaning;
            if (!result.TryFindEntity("StatusCodeMeaning", out statuscodemeaning))
            {
                await context.PostAsync("Sorry I don't know what you mean. My bad!");
                context.Wait(this.MessageReceived);
                return;
            }

            var meanings = result.Entities.Select(x => x.Entity);
            var results = StatusCodeList.Where(x => meanings.All(y => x.meaning.ToLower().Contains(y.ToLower())));

            await context.PostAsync("I think what you're after is...");

            foreach (var statuscodeInfo in results)
            {
                await context.PostAsync($"Status Code : {statuscodeInfo.code} Meaning : {statuscodeInfo.meaning} Description : {statuscodeInfo.description} {statuscodeInfo.imageurl}");

            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GreetingIntent")]
        public async Task HandleGreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi there! How can I help?");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GlennMillerIntent")]
        public async Task HandleGlenMillerIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have chosen wisely...");
            await context.PostAsync("https://www.youtube.com/watch?v=xPXwkWVEIIw");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        public async Task HandleNone(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("What you talking about Willis?");
            await context.PostAsync(result.Query + " makes no sense!");
            context.Wait(this.MessageReceived);
        }
    }
}
