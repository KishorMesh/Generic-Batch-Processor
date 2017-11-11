using System.Threading.Tasks;
using TaskExecuter.Shared.Messages;

namespace TaskExecuter.Shared.ExternalSystems
{
    /// <summary>
    /// Interface for client executable tasks
    /// </summary>
    public interface ITaskExecuter
    {
        Task<AcknowledgementMessage> ExecuteTask(JobStartedMessage task);
    }
}
