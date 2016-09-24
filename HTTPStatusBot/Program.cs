namespace HTTPStatusBot
{
    using Nancy.Hosting.Self;
    using System;

    public class Program
    {
        static void Main(string[] args)
        {

            var config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = true;
            config.UrlReservations.User = "Everyone";
            using (var host = new NancyHost(config, new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Running on http://localhost:1234");
                Console.ReadLine();
            }
        }
    }
}


