# Generic Batch Processor
”Building a concurrent and distributed System for batch processing which is fault tolerant and can scale up or scale out using [Akka.NET](http://getakka.net/ "Akka.NET - .NET distributed actor framework") (based on actor model)”. 

A generic batch processor is used for dividing parallel work to multiple actors/processes on the same machine, remote machine as well as in distributed network.

### Batch Processor workflow

![Image of Workflow](https://github.com/ERS-HCL/Generic-Batch-Processor/blob/master/workflow.PNG)


## Job Manager
Job Manger is managing all the tasks and their status. This can be UI or console based appliaction.
### Job Pool Controller Actor
Job Pool Controller Actor has following responsibilities
1. Assign the task to commander actor
2. Get the response from commander actor
3. update the job status
4. print the job statastics
 
### Job Scheduler
Job Scheduler is responsible for scheduling the job after each interval (10 sec ). It is also responsible for checking the job status after every 3 minutes interval.


## Executer
Executer is the backbone the batchprocessor acgtor system. It is responsible for taking the job from job manager (Job pool controller actor) and perfor that job and after completion, update job manager.
### Commander Actor
Commander actor has following responsibilities-
1. Create the broadcast router for creating coordinator instances.
2. Get the task from Job pool controller and assign it to any available coordinator.
3. Get the response from coordinator and update to job pool controller about the task.

### Coordinator Actor
Coordinator actor plays mediator role between commander and worker actor.Coordinator actor has following responsibilities-
1. Create the worker actor for performing task.
2. Supervise worker actor. If worker actor failed to perform any task then it will take necessary action.
3. Update the commander once the task is completed by worker.


### Worker Actor
Worker actor is the last actor in the hierarchy which actully perform the task. It has following responsibilities-
1. Get the task from coordinator and perform the task.
2. Update the task status to coordinator

## Current Samples
**[Fault Tolerant Concurrent application for Batch Processing](/Concurrent-Application/)** - Demostrates how to execute multiple tasks concurrently as well as paralley in Akka.NET.

**[Fault Tolerant Remote application for Batch Processing](/Remote-Application/)** - Demostrates how remotely perform multple tasks concurrently as well as parallely in Akka.NET. 

![Image of LocationTransparency](/Remote-Application/Location_Transparency.PNG)

**[Fault Tolerant Distributed application for Batch Processing](/Distributed-Application/)** - Demostrates how multple tasks performed on distributed systems in Akka.NET. 

![Image of Distributed System Workflow](/Distributed-Application/Distribute_Flow_Diagram.PNG)

## Build Instructions


## Contributing

Please feel free to contribute.

### Questions about Samples?

Please [create a Github issue] (https://github.com/ERS-HCL/Generic-Batch-Processor/issues) for any questions you might have.

### Code License
MIT


## Docs
Detailed documentaion about Akka.NET is available at [Akka.NET docs](http://getakka.net/).

## Tools / prerequisites
This course expects the following:
- You have some programming experience and familiarity with C#

## Author
Vijaykumar Thombre
Vijaykumar.Thombre@Hcl.com