using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IGlobalCommercialInvoiceJobProvider
	{
		ZGuid ParentID { get; }
		ZString ParentTableCode { get; }
		ZString JobNumber { get; }
		ZString VolumeUnitOfMeasurement { get; }
		ZString WeightUnitOfMeasurement { get; }
	}
}
