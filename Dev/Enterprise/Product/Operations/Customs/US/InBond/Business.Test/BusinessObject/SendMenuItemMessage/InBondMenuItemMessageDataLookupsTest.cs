using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondMenuItemMessageDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFIRMSCollection()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, null, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageData.USDestinationPortCode = "1234";
			var collection = inBondMenuItemMessageData.Lookups.FIRMSCollection;
			AssertNotNull(collection);
			AssertEquals("1234", collection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue && x.PropertyName == "Property").Value);
		}

		public void TestRegionDistrictPorts()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, null, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var collection = inBondMenuItemMessageData.Lookups.RegionDistrictPorts;
			AssertNotNull(collection);
		}

		public void TestExportTransportModeCodes()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, null, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			AssertEquals(inBondMenuItemMessageData.Lookups.TransportModeCodes.Count, 2);
			Assert(inBondMenuItemMessageData.Lookups.TransportModeCodes.ContainsCode(InBondTransportModeCodes.Codes.VesselContainer));
			Assert(inBondMenuItemMessageData.Lookups.TransportModeCodes.ContainsCode(InBondTransportModeCodes.Codes.VesselNonContainer));
		}

		public void TestConveyanceList()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, null, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			RefVesselCollection collection = inBondMenuItemMessageData.Lookups.ConveyanceList;
			AssertNotNull(collection);
		}
	}
}
