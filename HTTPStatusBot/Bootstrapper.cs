﻿namespace HTTPStatusBot
{
    using System;
    using Nancy;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.OnError += (ctx, ex) =>
            {
                ctx.Items.Add("OnErrorException", ex);
                return null;
            };

            pipelines.AfterRequest += (ctx) =>
            {
                //Console.WriteLine("after");
            };
        }
    }
}

