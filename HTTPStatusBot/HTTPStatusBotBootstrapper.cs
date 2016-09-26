namespace HTTPStatusBot
{
    using System;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;

    public class HTTPStatusBotBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.OnError += (ctx, ex) =>
            {
                //Console.Error(ex.ToString());
                return null;
            };
        }
    }
}