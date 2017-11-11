using Akka.Actor;
using System.Collections.Generic;
using TaskExecuter.Messages;
using System;
using System.IO;
using TaskExecuter;
using TaskExecuter.Actors;
using Akka.Routing;

namespace TaskExecuter.Actors
{
    public class JobPoolControllerActor : ReceiveActor
    {
        private class UpdateJobStatastics { }

        private class ProcessJob { }

        List<ProcessJobMessage> _jobsToProcessed = new List<ProcessJobMessage>();
        List<JobCompletedMessage> _succeedJobs = new List<JobCompletedMessage>();
        List<JobFailedMessage> _failedJobs = new List<JobFailedMessage>();

        /// <summary>
        /// scheduler instance for processing unstashed objects
        /// </summary>
        private ICancelable _jobScheduler;

        private IActorRef _commander;

        ProcessJobMessage _currentJobMsg;

        public JobPoolControllerActor(IActorRef commanderActor)
        {
            _commander = commanderActor;
            Receive<ProcessFileMessage>(msg => InitializeJobs(msg.FileName));
            Receive<ProcessJob>(job => HandleProcessJob());

            Receive<JobValidationSucceedMessage>(msg =>
            {
                _commander.Tell(new CanAcceptJobMessage(_currentJobMsg.Description, _currentJobMsg.ID));
            });

            Receive<JobValidationFailedMessage>(msg =>
            {
                ColorConsole.WriteLineRed("Invalid Task : {0} {1}", _currentJobMsg.ID, _currentJobMsg.Description);
            });

            Receive<AbleToAcceptJobMessage>(job =>
            {
                ColorConsole.WriteLineGreen("Commander {0} is able to accept job {1}. {2}", _commander.Path.Name, job.ID, job.Description);
                _commander.Tell(new BeginJobMessage(job.Description, job.ID));
            });

            Receive<UnableToAcceptJobMessage>(job =>
            {
                ColorConsole.WriteLineGreen("Commander {0} is unable to accept job {1}. {2}", _commander.Path.Name, job.ID, job.Description);
            });

            Receive<JobCompletedMessage>(job => HandleJobCompleted(job));
            Receive<JobFailedMessage>(job => HandleJobFailed(job));
            Receive<UpdateJobStatastics>(message => HandleUpdateJobStatastics());            
        }

        private void HandleUpdateJobStatastics()
        {
            ColorConsole.WriteLineMagenta("Total # of jobs to be processed are {0}", _jobsToProcessed.Count);
            ColorConsole.WriteLineMagenta("Total # of succeed jobs are {0}", _succeedJobs.Count);
            ColorConsole.WriteLineMagenta("Total # of failed jobs are {0}", _failedJobs.Count);            
        }
        
        private void HandleJobCompleted(JobCompletedMessage job)
        {
            _succeedJobs.Add(job);
            ColorConsole.WriteLineGreen("Task {0}. {1} completed succesfully in {2} ms", job.ID, job.Description, job.Duration);
            Self.Tell(new UpdateJobStatastics());
        }

        private void HandleJobFailed(JobFailedMessage job)
        {
            _failedJobs.Add(job);
            ColorConsole.WriteLineRed("Task {0}. {1} failed ", job.ID, job.Description);
            Self.Tell(new UpdateJobStatastics());           
        }              
        
        private void HandleProcessJob()
        {
            if (_jobsToProcessed.Count > 0)
            {
                _currentJobMsg = _jobsToProcessed[0];
                Context.ActorSelection(ActorPaths.ValidatorActor.Path).Ask(_currentJobMsg.Description).PipeTo(Self);              
                _jobsToProcessed.RemoveAt(0);
            }
            else
            {
                ColorConsole.WriteLineYellow("All jobs are processed from job pool queue..");
                 _jobScheduler.Cancel();               
            }          
        }
        
        private void InitializeJobs(string fileName)
        {
            ParseTxtFile(fileName);
            ColorConsole.WriteLineCyan("Initializing Job Pool...");
            ColorConsole.WriteLineCyan("-----------------------------------------");
            foreach(var job in _jobsToProcessed)
            {
                ColorConsole.WriteLineWhite("Task {0}. {1}", job.ID, job.Description);
            }

            ColorConsole.WriteLineCyan("-----------------------------------------");
            ColorConsole.WriteLineWhite("Total tasks to be processed are : {0}", _jobsToProcessed.Count);
            ColorConsole.WriteLineCyan("=========================================");
        }

        private void ParseTxtFile(string fileName)
        {
            bool fileFound = File.Exists(fileName);
            if (!fileFound)
            {
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);
                fileFound = File.Exists(fileName);
                if (!fileFound)
                {
                    ColorConsole.WriteLineRed("Task file not found.");
                    return;
                }
            }
                       
            var fileLines = File.ReadAllLines(fileName);
            int jobId = 0;
            foreach (var line in fileLines)
            {
                jobId++;
                var job = new ProcessJobMessage(line, jobId);
                _jobsToProcessed.Add(job);
            }
        }

        #region Lifecycle Event Hooks
        protected override void PreStart()
        {
           // _commander = Context.ActorOf(Props.Create(() => new CommanderActor())
             //               /* .WithRouter(FromConfig.Instance)*/, ActorPaths.CommanderActor.Name);

            _jobScheduler = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(10),
                Self,
                new ProcessJob(),
                Self);
            base.PreStart();
        }
        
        protected override void PostStop()
        {
            _jobScheduler.Cancel();
        }

        #endregion
    }
}
