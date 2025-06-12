using System.Threading.Tasks;

namespace OcmPoc.Core.Services
{
	public interface IMessageFlowCorrelator
	{
		Task CorrelateFlowAsync(int messageFlowId, string sender, string recipient, string correlationId);
	}
}
