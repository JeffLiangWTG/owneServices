using System;
using System.Threading.Tasks;

namespace Enterprise.Freight.AIS
{
	public interface IAisWebApiClient : IDisposable
	{
		Task<PortCallPage> PortCallsAsync(string vesselImo, int? pageIndex, int? pageSize, string timeFrom, string timeTo, string carrierCode, string voyageNumber, string departurePortUnloco, string arrivalPortUnloco, System.Threading.CancellationToken cancellationToken);
	}
}
