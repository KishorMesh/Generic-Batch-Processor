using Akka.Actor;
using System;
using API.Exceptions;
using API.ExternalSystems;
using API.Messages;

namespace API.Actors
{
    internal class WorkerActor: ReceiveActor
    {
        #region private members
      
        /// <summary>
        /// executer instance
        /// </summary>
        private readonly ITaskExecuter _taskExecuter;
       
        /// <summary>
        /// instance of BeginJobMessage
        /// </summary>
        private JobStartedMessage _myJob;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerActor"/>  class
        /// </summary>
        /// <param name="executer">Executer object</param>
        public WorkerActor(ITaskExecuter executer)
        {            
            _taskExecuter = executer;

            Receive<JobStartedMessage>(job => HandleJobExecute(job));
            Receive<AcknowledgementMessage>(message => HandleAcknowldgement(message));            
        }

        #region Handle Receive Messages
        /// <summary>
        /// Perform the exceution of job, Handles "BeginJobMessage" message
        /// </summary>
        /// <param name="job"></param>
        private void HandleJobExecute(JobStartedMessage job)
        {
            _myJob = job;
            // use pipeto to handle async call from caller ( taskexecuter )
            _taskExecuter.ExecuteTask(job).PipeTo(Self, Sender);
        }

        /// <summary>
        /// Perform acknowledgement message from the sender.
        /// </summary>
        /// <param name="message"></param>
        private void HandleAcknowldgement(AcknowledgementMessage message)
        {
            if (message.Receipt == AcknowledgementReceipt.CANCELED)
            {
                 throw new JobCanceledException();
            }
            else if (message.Receipt == AcknowledgementReceipt.INVALID_TASK)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Failed));
                ColorConsole.WriteLineRed($"Task {message.ID}. {message.Description} is invalid.");
            }
            else if (message.Receipt == AcknowledgementReceipt.FAILED)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID,JobStatus.Failed));
                ColorConsole.WriteLineRed($"Task {message.ID}. { message.Description} is failed due to unhandled exeption.");
            }
            else if (message.Receipt == AcknowledgementReceipt.TIMEOUT)
            {
                Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Timeout));
                ColorConsole.WriteLineRed("Task ID: {0} is cancelled due to time out error.", message.ID);
            }
            else
            {
                if (message.CompletionTime == 0)
                {
                    ColorConsole.WriteLineCyan($"Task ID: {message.ID} failed to execute by external application.");
                    Context.Parent.Tell(new JobFailedMessage(message.Description, message.ID, JobStatus.Cancelled));                    
                }
                else
                {
                    ColorConsole.WriteLineCyan("Task ID: {0} completed successfully by worker.", message.ID);
                    Context.Parent.Tell(new JobCompletedMessage(message.Description, message.ID, message.CompletionTime));
                }
            }
        }
        #endregion

        #region Lifecycle hooks

        protected override void PreStart()
        {           
        }

        protected override void PostStop()
        {
            ColorConsole.WriteLineRed("WorkerActor for Coordinator {0} called PostStop.", Context.Parent.Path.Name);           
        }

        protected override void PreRestart(Exception reason, object message)
        {
            ColorConsole.WriteLineWhite("WorkerActor for Coordinator {0} called PreReStart because: {1}", Context.Parent.Path.Name, reason.Message);          
            Self.Tell(_myJob);
        }

        protected override void PostRestart(Exception reason)
        {            
            base.PostRestart(reason);
        }
        #endregion
    }
}
