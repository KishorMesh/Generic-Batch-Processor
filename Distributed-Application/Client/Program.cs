using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using API;
using API.Messages;

namespace Client
{
   class Program
   {
        private static void Main(string[] args)
        {
            var system = ActorSystem.Create("batchProcessor");

            var api = system.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "api");

            IActorRef jobPoolControllerActor = system.ActorOf(
                Props.Create<JobPoolControllerActor>(api), "jobpool");

            jobPoolControllerActor.Tell(new ProcessFileMessage("JobPool.txt"));

            system.WhenTerminated.Wait();
        }
   }

   /// <summary>
   /// Prints recommendations out to the console
   /// </summary>
   //public class Printer : ReceiveActor
   //{
   //   public Printer()
   //   {
   //      Receive<Recommendation>(res =>
   //      {
   //         var results = string.Join(Environment.NewLine, res.RecommendedVideos.Select(x => x.Title));

   //         Console.ForegroundColor = ConsoleColor.Green;
   //         Console.WriteLine($"Recommendations: {Environment.NewLine}{results}");
   //         Console.ResetColor();
   //      });
   //   }
   //}
}
