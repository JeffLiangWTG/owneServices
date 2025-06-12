using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageInterfaces.Tracking
{
	public interface IEventLogger
	{
		Task<long> LogEventAsync(MessageEvent messageEvent);
		Task<long> LogAsync(MessageEvent messageEvent);
	}
}
