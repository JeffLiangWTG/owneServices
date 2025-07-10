using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.Freight.Business
{
	public abstract class TransportSupporterCommon
	{
		public abstract ZString Description { get; }
		public abstract ZString ConsignmentRef { get; }
		public abstract ZString TransportMode { get; }
		public abstract ZString ContainerMode { get; }
		public abstract ZString BillOfLading { get; }
		public abstract SecurityCheckpoint DistanceCalculationCheckpoint { get; }

		public virtual bool CreateSailingIfNotExistsForUniversalShipment
		{
			get { return true; }
		}
	}
}
