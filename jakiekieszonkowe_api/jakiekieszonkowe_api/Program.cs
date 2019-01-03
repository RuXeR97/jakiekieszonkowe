using jakiekieszonkowe_api.Other;
using System;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace jakiekieszonkowe_api
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");
            //Guid id = Guid.NewGuid();
            //Guid id2 = Guid.NewGuid();

            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));
            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });
            config.MessageHandlers.Add(new CustomHeaderHandler());


            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }


    }
}
