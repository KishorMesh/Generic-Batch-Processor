using Akka.Actor;
using TaskExecuter.Shared;

namespace TaskExecuter.Remote
{
    class Program
    {
        private static ActorSystem TaskExecuterActorSystem;
        static void Main(string[] args)
        {
            ColorConsole.WriteLineGray("Creating Batch processor system on remote process  at node localhost:8090");     
            TaskExecuterActorSystem = ActorSystem.Create("batchprocessor");
            TaskExecuterActorSystem.WhenTerminated.Wait();
        }
    }
}
