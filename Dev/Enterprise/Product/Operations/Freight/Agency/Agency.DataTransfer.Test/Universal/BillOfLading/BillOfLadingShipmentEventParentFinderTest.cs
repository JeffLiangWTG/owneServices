using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class BillOfLadingShipmentEventParentFinderTest : AgencyShipmentEventParentFinderTest<BillOfLading>
	{
		protected override IEventDataContextManager GetNewEventDataContextManager()
		{
			return new BillOfLadingDataContextManager();
		}

		protected override ZString GetShipmentType()
		{
			return "Bill of Lading";
		}
	}
}
