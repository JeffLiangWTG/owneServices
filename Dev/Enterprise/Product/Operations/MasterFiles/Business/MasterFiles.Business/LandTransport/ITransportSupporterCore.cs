using CargoWise.Types;
using Enterprise.Integration.Schedule;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransportParentCore : ITransportParentCommon
	{
		ZString Description { get; }
		ZString ConsignmentRef { get; }
		ZString TransportMode { get; }
		ZString ContainerMode { get; }
		ZString BillOfLading { get; }
		SecurityCheckpoint DistanceCalculationCheckpoint { get; }
	}
}
