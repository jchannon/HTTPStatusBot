namespace HTTPStatusBot
{
    using Nancy.Hosting.Self;
    using System;

    public class Program
    {
        static void Main(string[] args)
        {

            using (var host = new NancyHost(new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Running on http://localhost:1234");
                Console.ReadLine();
            }
        }
    }
}


