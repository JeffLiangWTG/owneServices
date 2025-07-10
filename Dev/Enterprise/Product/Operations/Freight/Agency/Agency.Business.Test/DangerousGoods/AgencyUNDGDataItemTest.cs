using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyUNDGDataItem))]
	internal class AgencyUNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAgencyUNDGDataItemValidation()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			var agencyUNDGDataItem = collection.AddNew();
			AssertNotEquals(null, agencyUNDGDataItem.Validation);
			AssertType<AgencyUNDGDataItemValidation>(agencyUNDGDataItem.Validation);
		}
	}
}
