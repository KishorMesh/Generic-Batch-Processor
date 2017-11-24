using System;
using Akka.Actor;
using API.Exceptions;
using API.Messages;
using API.ExternalSystems;

namespace API.Actors
{
    public class CoordinatorActor : ReceiveActor
    {
        #region private members
        /// <summary>
        /// worker instance actor
        /// </summary>
        private IActorRef _taskWorker;

        /// <summary>
        /// parent 'commander' instance actor
        /// </summary>
        private IActorRef _parent;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinatorActor"/>  class
        /// </summary>
        public CoordinatorActor()
        {
            Waiting();
        }

        #region Switchable behaviour for Coordinator
        private void Waiting()
        {
             ColorConsole.WriteLineYellow("Coordinator {0} state is - Waiting.", Self.Path.Name);

            // Received CanAcceptJobMessage from coordinator
            Receive<CanAcceptJobMessage>(job =>
            {
                Sender.Tell(new AbleToAcceptJobMessage(job.Description, job.ID));
            });

            // Received BeginJobMessage from coordinator
            Receive<JobStartedMessage>(job =>
            {
                ColorConsole.WriteLineGreen("Task {0} is processing by coordinator {1}.", job.ID,Self.Path.Name);
                // move to next state first
                BecomeWorking();

                _parent = Sender;
                // ask the worker for job
                _taskWorker.Tell(job);
            });
        }

        private void BecomeWorking()
        {
            Become(Working);
        }

        private void Working()
        {
           // ColorConsole.WriteLineYellow("Cordinator {0}'s current state is Working", Self.Path.Name);

            // Received CanAcceptJobMessage from commander
            Receive<CanAcceptJobMessage>(job =>
            {               
                ColorConsole.WriteLineYellow("Coordinator {0} state is - Working.", Self.Path.Name);

                // send the response to commander
                Sender.Tell(new UnableToAcceptJobMessage(job.Description, job.ID));
            });

            // recieved JobCompletedMessage from worker
            Receive<JobCompletedMessage>(job => HandleJobCompleted(job));

            // recieved JobFailedMessage from worker
            Receive<JobFailedMessage>(job => HandleJobFailed(job));
        }

        private void BecomeWaiting()
        {
            Become(Waiting);
        }

        #endregion

        #region Handle Receive Messages

        /// <summary>
        /// Handle the message "HandleJobCompleted" received from Worker
        /// </summary>
        /// <param name="job"></param>
        private void HandleJobCompleted(JobCompletedMessage job)
        {
            ColorConsole.WriteLineMagenta("Task {0} completed by Coordinator {1}.", job.ID, Self.Path.Name);
           
            // send response to parent -commander            
           _parent.Tell(job);
            
            // move to next state
            BecomeWaiting();
        }

        /// <summary>
        /// Handle the message "JobFailedMessage" received from Worker
        /// </summary>
        /// <param name="job">message</param>
        private void HandleJobFailed(JobFailedMessage job)
        {
            ColorConsole.WriteLineMagenta("Task {0} of Coordinator {1} failed.", job.ID, Self.Path.Name);

            // send response to parent - commander            
            _parent.Tell(job);
            // move to next state
            BecomeWaiting();
        }

        #endregion

        /// <summary>
        /// Supervisor strategy for coordinator
        /// </summary>
        /// <returns>action taken by the coordinator</returns>
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                exception =>
                {
                    if (exception is JobCanceledException)
                    {
                        ColorConsole.WriteLineRed("TaskCanceledException caught for Coordinator {0}, Restarting the task...", Self.Path.Name);
                        return Directive.Restart;
                    }
                    if (exception is UnHandledException)
                    {
                        ColorConsole.WriteLineRed("Unhandled exception caught for Coordinator {0}, Resuming Task...", Self.Path.Name);
                        return Directive.Resume;
                    }                    

                    ColorConsole.WriteLineRed("Unknown exception caught for Coordinator {0}, Restarting the task...", Self.Path.Name);
                    return Directive.Restart;
                });
        }

        #region Lifecycle Hooks
        protected override void PreStart()
        {
           ColorConsole.WriteLineBlue("Cordinator{0}'s PreStart called.", Self.Path.Name);

            //_taskWorker = Context.ActorOf(Context.DI().Props<WorkerActor>(),
              //  ActorPaths.WorkerActor.Name);
            
            ITaskExecuter clientExecuter = new ClientTaskExecuter();
            _taskWorker = Context.ActorOf(Props.Create<WorkerActor>(clientExecuter), "worker");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineWhite("Cordinator{0}'s PreRestart called because: {1} ", Self.Path.Name, reason.Message);
            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineRed("Cordinator{0}'s stopped.", Self.Path.Name);
        }

        protected override void PostRestart(Exception reason)
        {
            ColorConsole.WriteLineBlue("Cordinator{0}'s PostRestart called because: {1} ", Self.Path.Name, reason.Message);
            base.PostRestart(reason);
        }
        #endregion
    }
}
