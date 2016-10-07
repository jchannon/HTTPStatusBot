namespace HTTPStatusBot.StatusCodeHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nancy;
    using Nancy.ErrorHandling;
    using Nancy.Responses;

    public class ErrorStatusCodeHandler : IStatusCodeHandler
    {
        private readonly IEnumerable<ISerializer> serializers;

        public ErrorStatusCodeHandler(IEnumerable<ISerializer> serializers )
        {
            this.serializers = serializers;
        }
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (context.Items.ContainsKey("OnErrorException"))
            {
                var exception = context.Items["OnErrorException"] as Exception;
                var error = new  { ErrorMessage = exception.Message, FullException = exception.ToString() };


                context.Response = new JsonResponse(error,
                    this.serializers.FirstOrDefault(x => x.CanSerialize("application/json")), context.Environment);
            }
        }
    }
}