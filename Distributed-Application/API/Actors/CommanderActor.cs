using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskExecuter.Messages;

namespace TaskExecuter.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching task-processing jobs
    /// </summary>
    public class CommanderActor : ReceiveActor, IWithUnboundedStash
    {
        #region private members
        /// <summary>
        ///  class for processing stashed jobs
        /// </summary>
        private class ProcessStashedJobs { }

        /// <summary>
        /// Coordinator instance actor
        /// </summary>
        private IActorRef _coordinator;

        
        /// <summary>
        /// scheduler instance for processing unstashed objects
        /// </summary>
       // private ICancelable _unstashSchedule;        

        /// <summary>
        /// no. of routees replies
        /// </summary>
        private int _pendingJobReplies;

        /// <summary>
        /// current job id to be processed
        /// </summary>
        private int _currentJobID;

        /// <summary>
        /// current job description to be processed
        /// </summary>
        private string _currentJobDescription;

        /// <summary>
        /// container for stashed job
        /// </summary>
        private List<int> stashedJobs = new List<int>();
        #endregion

        /// <summary>
        /// gets or sets the Stash
        /// </summary>
        public IStash Stash { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommanderActor"/>  class
        /// </summary>
        public CommanderActor()
        {
            Ready();
        }

        #region Switchable behaviour for Coordinator
        private void Ready()
        {
            ColorConsole.WriteLineGreen("Commander's current state is Ready.");

            Receive<CanAcceptJobMessage>(job =>
            {
                // ask the coordinator for job
                _coordinator.Tell(job);
                _currentJobID = job.ID;
                _currentJobDescription = job.Description;                

                // move to next state              
                BecomeAsking();
            });

            Receive<JobCompletedMessage>(job =>
            {
                // send response to parent
                Context.Parent.Tell(job);                         
                
                // move to next state
                BecomeReady();
            });

            Receive<JobFailedMessage>(job =>
            {
                // send response to parent
                Context.Parent.Tell(job);
               
                // move to next state
                BecomeReady();
            });

            // recived ProcessStashedJobs request from scheduler
           // Receive<ProcessStashedJobs>(job => HandleUnstashJobs());
        }

        private void BecomeAsking()
        {
            // block, but ask the router for the number of routees. Avoids magic numbers.
            _pendingJobReplies = _coordinator.Ask<Routees>(new GetRoutees()).Result.Members.Count();

            // move to next state
            Become(Asking);

            // send ourselves a ReceiveTimeout message if no message within 3 seonds
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(3));
        }

        private void Asking()
        {
            // stash any subsequent requests
            Receive<CanAcceptJobMessage>(job =>
            {
                //if (!stashedJobs.Contains(job.ID))
                //{
                //    ColorConsole.WriteLineWhite("Stashing job into waiting queue for Task ID: {0}", job.ID);
                //    stashedJobs.Add(job.ID);
                //    Stash.Stash();
                //}

            });

            // received UnableToAcceptJobMessage from coordinator
            Receive<UnableToAcceptJobMessage>(job =>
            {
                //Context.Parent.Tell(job);
                //BecomeAsking();
                // each routee is giving response to cordinator
                _pendingJobReplies--;
                if (_pendingJobReplies == 0)
                {
                    // send response to parent
                    Context.Parent.Tell(job);

                    //// send response to ourself for stashing job, since it is is waiting queue
                    //Self.Tell(new CanAcceptJobMessage(job.Description, job.ID));

                    // move to next state
                    BecomeAsking();
                }
            });

            // received AbleToAcceptJobMessage from coordinator
            Receive<AbleToAcceptJobMessage>(job =>
            {
                // send response to parent
                Context.Parent.Tell(job);

                 // ask for processing messages to coordinator
                 Sender.Tell(new BeginJobMessage(job.Description, job.ID));

                BecomeReady();
            });

            // means at least one actor failed to respond
            Receive<ReceiveTimeout>(timeout =>
            {
                // send response to parent
                Context.Parent.Tell(new UnableToAcceptJobMessage(_currentJobDescription, _currentJobID));

                // move to next state
                BecomeWorking();
            });
        }

        private void BecomeReady()
        {
            // cancel ReceiveTimeout
            Context.SetReceiveTimeout(null);

            // move to next state
            Become(Ready);
        }

        private void BecomeWorking()
        {
            Become(Working);
        }

        private void Working()
        {

        }



        #endregion

        #region Handle Receive Messages

        /// <summary>
        /// Handle all unstashed jobs
        /// </summary>
        private void HandleUnstashJobs()
        {
            Stash.UnstashAll();
            this.stashedJobs.Clear();
        }
        #endregion

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                exception =>
                {
                    ColorConsole.WriteLineRed("Unknown exception caugh for commander, restarting the task...");                   
                    return Directive.Restart;
                });
        }

        #region Lifecycle Event Hooks
        protected override void PreStart()
        {           
            // create a broadcast router who will ask all of them if they're available for work
            _coordinator = Context.ActorOf(Props.Create(() => new CoordinatorActor())
                .WithRouter(FromConfig.Instance), ActorPaths.CoordinatorActor.Name);

            //_unstashSchedule = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
            //    TimeSpan.FromSeconds(60),
            //    TimeSpan.FromSeconds(20),
            //    Self,
            //    new ProcessStashedJobs(),
            //    Self);
            base.PreStart();
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineRed("Commander's PostStop called.");
           // _unstashSchedule.Cancel();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineWhite("Commander's PreRestart called because: {0} ", reason.Message);           

            //kill off the old coordinator so we can recreate it from scratch
            ColorConsole.WriteLineWhite("kill off the old coordinator so we can recreate it from scratch");
            _coordinator.Tell(PoisonPill.Instance);

            base.PreRestart(reason, message);
        }

        #endregion
    }
}
