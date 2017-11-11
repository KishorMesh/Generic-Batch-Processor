using Akka.Actor;
using TaskExecuter.Messages;
using System.IO;

namespace TaskExecuter.Actors
{
    /// <summary>
    /// Actor that validates user input and signals result to others.
    /// </summary>
    public class ValidatorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is string)
            {
                if (ValidateProcessedTask(message as string))
                {
                    Sender.Tell(new JobValidationSucceedMessage());
                }
                else
                {
                    Sender.Tell(new JobValidationFailedMessage());
                }
            }
        }

        /// <summary>
        /// Validate task
        /// </summary>
        /// <param name="message">task message</param>
        private bool ValidateProcessedTask(string message)
        {
            bool isValid = false;
            if (string.IsNullOrEmpty(message))
            {               
                isValid = false;
            }
            else
            {                
                isValid = validateTaskFiles(message);
            }
            return isValid;
        }
        /// <summary>
        /// Checks if file exists at path provided by user.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Validates filepaths for the task
        /// </summary>
        /// <param name="fileName">filename</param>
        /// <returns>true if valid paths, else false</returns>
        private bool validateTaskFiles(string fileName)
        {
            bool isValid = false;
            var fileLines = File.ReadAllLines(fileName);
            if (fileLines.Length > 1)
            {
                string compartmentExtractorFile = fileLines[0];
                if (IsFileUri(compartmentExtractorFile))
                {
                    string childExtractorFile = fileLines[1];
                    if (IsFileUri(childExtractorFile))
                    {
                        isValid = true;
                    }
                }
            }
            return isValid;
        }
    }
}
