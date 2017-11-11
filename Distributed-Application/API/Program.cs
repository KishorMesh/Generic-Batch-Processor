using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;

namespace API
{
   public class Program
   {
      private static void Main(string[] args)
      {
         ActorSystem system = ActorSystem.Create("batchProcessor");
       
         system.ActorOf(Props.Create(() => new API()), "api");

         system.WhenTerminated.Wait();
      }
   }
}