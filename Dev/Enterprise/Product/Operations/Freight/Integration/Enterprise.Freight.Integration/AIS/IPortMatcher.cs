using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Freight.Integration
{
	public interface IPortMatcher : IDisposable
	{
		Task<IPortMatches> MatchAsync(IVesselMovementsUrlModel request, CancellationToken cancellationToken = default);
	}
}
