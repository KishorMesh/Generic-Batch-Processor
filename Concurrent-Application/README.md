## Concurrent application for batch processing which is fault tolerant using [Akka.NET](http://getakka.net/ "Akka.NET - .NET distributed actor framework"). 
Demonstrates via a console application how `concurrent as well as parallely perform multple tasks`. 

## Workflow
![Image of Workflow](/Concurrent-Application/Workflow.PNG)



## Running the Sample
1. Open `TaskExecuter.sln` in Visual Studio 2015 or later.
2. Open ClientTaskExceuter.cs file and modify ExecuteTask() method for you application.
3. Provide the job details in JobPool.txt file
4. Press `F6` to build the sample - this solution has [NuGet package restore](http://docs.nuget.org/docs/workflows/using-nuget-without-committing-packages) enabled, so any third party dependencies will automatically be downloaded and added as references.
5. Press `F5` to run the sample.


## Contributing

Please feel free to contribute.

### Questions about Samples?

If you have any questions about this sample, please [Create a Github issue for us](https://github.com/ERS-HCL/Generic-Batch-Processor/issues)!

### Code License
MIT








