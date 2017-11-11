using System;
using Akka.Actor;
using System.Threading;
using TaskExecuter.Shared;
using TaskExecuter.Shared.Actors;
using TaskExecuter.Shared.Messages;

namespace TTaskExecuter.Deployer
{
    internal class Program
    {
        private static ActorSystem TaskExecuterActorSystem;

        private static void Main(string[] args)
        {
            ColorConsole.WriteLineWhite("Creating Batch processor system at node localhost:8091");
            TaskExecuterActorSystem = ActorSystem.Create("batchprocessor");       

            IActorRef commanderActor = TaskExecuterActorSystem.ActorOf(Props.Create<CommanderActor>(),
             ActorPaths.CommanderActor.Name);

            IActorRef jobPoolControllerActor = TaskExecuterActorSystem.ActorOf(Props.Create<JobPoolControllerActor>(commanderActor),
               ActorPaths.JobPoolControllerActor.Name);

            jobPoolControllerActor.Tell(new ProcessFileMessage("JobPool.txt"));

            TaskExecuterActorSystem.WhenTerminated.Wait();
        }

        //private static void CreateActorSystem()
        //{
        //    // Ninject Dependency Injector
        //    var container = new StandardKernel();
        //    container.Bind<ITaskExecuter>().To<ClientTaskExecuter>();
        //    container.Bind<WorkerActor>().ToSelf();

        //    TaskExecuterActorSystem = ActorSystem.Create("batchprocessor");

        //    IDependencyResolver resolver = new NinjectDependencyResolver(container, TaskExecuterActorSystem);
        //}

    }
}
