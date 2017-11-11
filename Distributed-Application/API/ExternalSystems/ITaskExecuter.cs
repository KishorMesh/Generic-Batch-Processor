using System.Threading.Tasks;
using API.Messages;

namespace API.ExternalSystems
{
    /// <summary>
    /// Interface for client executable tasks
    /// </summary>
    public interface ITaskExecuter
    {
        Task<AcknowledgementMessage> ExecuteTask(JobStartedMessage task);
    }
}
