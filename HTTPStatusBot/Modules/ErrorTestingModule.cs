namespace HTTPStatusBot.Modules
{
    using System;
    using Nancy;
    public class ErrorTestingModule : NancyModule
    {
        public ErrorTestingModule()
        {
            Get("/", _ =>
            {
                throw new Exception("oops");
                return 200;
            });
        }
    }
}
