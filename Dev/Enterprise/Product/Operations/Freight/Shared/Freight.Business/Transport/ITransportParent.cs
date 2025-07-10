using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface ITransportParent : IImportExport, ITransportChangeNotifier, ITransportParentCommon
	{
		TransportSupporter TransportSupporter { get; }
		TransportCollection Transports { get; }
	}
}
