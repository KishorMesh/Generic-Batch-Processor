namespace TaskExecuter.Shared
{
    /// <summary>
    /// Static helper class used to define paths to fixed-name actors
    /// (helps eliminate errors when using <see cref="ActorSelection"/>)
    /// </summary>
    public static class ActorPaths
    {
        public static readonly ActorMetaData JobPoolControllerActor 
            = new ActorMetaData("jobcontroller", "akka.tcp://batchprocessor@localhost:8091/user/jobcontroller");

        public static readonly ActorMetaData CommanderActor     
            = new ActorMetaData("commander", "akka.tcp://batchprocessor@localhost:8091/user/commander");

        public static readonly ActorMetaData CoordinatorActor   
            = new ActorMetaData("coordinator", "akka.tcp://batchprocessor@localhost:8091/user/commander/coordinator");

        public static readonly ActorMetaData WorkerActor        
            = new ActorMetaData("worker", "akka.tcp://batchprocessor@localhost:8091/user/commander/coordinator/worker");
    }

    /// <summary>
    /// Meta-data class
    /// </summary>
    public class ActorMetaData
    {
        public ActorMetaData(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; private set; }

        public string Path { get; private set; }
    }
}
