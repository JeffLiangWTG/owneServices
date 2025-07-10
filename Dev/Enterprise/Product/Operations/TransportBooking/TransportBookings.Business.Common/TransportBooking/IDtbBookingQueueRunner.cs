using System.Threading;
using Enterprise.Integration;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbBookingQueueRunner
	{
		void Run(CancellationToken token, ILogger logger);
	}
}
