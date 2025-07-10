using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDetachedConsignments_DefaultFilters()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();

			var detachedConsignments = consignmentHeader.Lookups.DetachedConsignment_List;
			var defaultFilters = detachedConsignments.FilterBusinessObjectDefaults;
			AssertEquals(3, defaultFilters.Count);
			var relatedShipmentFilter = defaultFilters.Cast<FilterBusinessObjectDefault>().FirstOrDefault(x => x.FilterName == "Shipments");
			AssertEquals(consignmentHeader.HCH_JS_Shipment, relatedShipmentFilter.Value);
			var attachedStatusFilter = defaultFilters.Cast<FilterBusinessObjectDefault>().FirstOrDefault(x => x.FilterName == "Consignment Status");
			AssertEquals("DTC", attachedStatusFilter.Value);
			var activeStatusFilter = defaultFilters.Cast<FilterBusinessObjectDefault>().FirstOrDefault(x => x.FilterName == "Active Status");
			AssertEquals("Inactive", activeStatusFilter.Value);
		}
	}
}
