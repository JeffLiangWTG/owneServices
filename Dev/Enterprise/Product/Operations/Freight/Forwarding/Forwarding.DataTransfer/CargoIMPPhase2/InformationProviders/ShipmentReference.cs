using CargoWise.Types;
namespace Enterprise.Freight.Forwarding.DataTransfer
{
	enum ShipmentReferenceTypes
	{
		None,
		JobNumber,
		MasterHouseBill,
		UniqueConsignmentReference
	}

	struct ShipmentReference
	{
		public InformationResult<ZString> Reference;
		public InformationResult<ShipmentReferenceTypes> ReferenceType;
	}
}
