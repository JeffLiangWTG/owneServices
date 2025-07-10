using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTradeTypes_CustomsBrokerage()
		{
			AssertContainsExactElementsInAnyOrder(
				new[] { OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import, OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export },
				OrgTradeDetailLookups.GetTradeTypes(SystemDefinedSalesProductList.Codes.CustomsBrokerage, "").Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestSupplierParts()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			var lookup = tradeDetail.Lookups;

			AssertEquals(false, lookup.SupplierParts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));

			var org = Factory.New<OrgHeader>();
			sales.OW_OH_Primary = org.PK;

			var filterBizObjDefault = lookup.SupplierParts.FilterBusinessObjectDefaults["Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"];
			AssertNotNull(filterBizObjDefault);
			AssertEquals(org.PK, filterBizObjDefault.Value);
		}

		public void TestGetTradeModes()
		{
			var customsBrokerage = OrgTradeDetailLookups.GetTradeModes(SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			AssertEquals("Collection should contain 5 records", 5, customsBrokerage.Count);
			AssertEquals("The CustomsBrokerage list should contain 'AIR'", true, customsBrokerage.ContainsCode(Constants.TransportModes.Air));
			AssertEquals("The CustomsBrokerage list should contain 'SEA'", true, customsBrokerage.ContainsCode(Constants.TransportModes.Sea));
			AssertEquals("The CustomsBrokerage list should contain 'RAI'", true, customsBrokerage.ContainsCode(Constants.TransportModes.Rail));
			AssertEquals("The CustomsBrokerage list should contain 'ROA'", true, customsBrokerage.ContainsCode(Constants.TransportModes.Road));
			AssertEquals("The CustomsBrokerage list should contain 'MAI'", true, customsBrokerage.ContainsCode(Constants.TransportModes.Mail));

			var forwardingShipment = OrgTradeDetailLookups.GetTradeModes(SystemDefinedSalesProductList.Codes.ForwardingShipment);
			AssertEquals("Collection should contain 5 records", 5, forwardingShipment.Count);
			AssertEquals("The ForwardingShipment list should contain 'AIR'", true, forwardingShipment.ContainsCode(Constants.TransportModes.Air));
			AssertEquals("The ForwardingShipment list should contain 'SEA'", true, forwardingShipment.ContainsCode(Constants.TransportModes.Sea));
			AssertEquals("The ForwardingShipment list should contain 'RAI'", true, forwardingShipment.ContainsCode(Constants.TransportModes.Rail));
			AssertEquals("The ForwardingShipment list should contain 'ROA'", true, forwardingShipment.ContainsCode(Constants.TransportModes.Road));
			AssertEquals("The ForwardingShipment list should contain 'COU'", true, forwardingShipment.ContainsCode(Constants.TransportModes.Courier));

			var linerAgency = OrgTradeDetailLookups.GetTradeModes(SystemDefinedSalesProductList.Codes.LinerAgency);
			AssertEquals("Collection should contain 1 record", 1, linerAgency.Count);
			AssertEquals("The LinerAgency list should contain 'BOL'", true, linerAgency.ContainsCode(OrgTradeDetailLookups.LinerAgencyTradeModes.BillOfLading));

			var transport = OrgTradeDetailLookups.GetTradeModes(SystemDefinedSalesProductList.Codes.Transport);
			AssertEquals("Collection should contain 1 record", 1, transport.Count);
			AssertEquals("The Transport list should contain 'TBK'", true, transport.ContainsCode(OrgTradeDetailLookups.TransportTradeModes.TransportBooking));
		}
	}
}
