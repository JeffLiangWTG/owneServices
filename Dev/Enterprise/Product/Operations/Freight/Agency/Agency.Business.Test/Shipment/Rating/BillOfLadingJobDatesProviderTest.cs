namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingJobDatesProviderTest : AgencyShipmentJobDatesProviderTest
	{
		protected override AgencyShipment GetAgencyShipment()
		{
			return Factory.New<BillOfLading>();
		}
	}
}
