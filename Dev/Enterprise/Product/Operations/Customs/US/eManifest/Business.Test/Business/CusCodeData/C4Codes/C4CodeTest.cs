using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(C4Code))]
	sealed class C4CodeTest : Customs.Business.Testing.CusCodeDataTest<C4Code>
	{
		public void TestSetDefaultValues()
		{
			var number = Factory.New<C4Code>();
			AssertEquals(CusCodeDataTypeList.Codes.C4Code, number.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.C4Code, number.CY_Code);
		}

		public void TestParent()
		{
			var commodity = Factory.New<Commodity>();
			var number = Factory.New<C4Code>();
			number.CY_ParentID = commodity.PK;
			number.CY_ParentTableCode = commodity.TablePrefix;
			AssertEquals(commodity, number.Parent);
			number = commodity.C4Codes.AddNew();
			AssertEquals(commodity.TablePrefix, number.CY_ParentTableCode);
			AssertEquals(commodity.PK, number.CY_ParentID);
			AssertEquals(commodity, number.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			var commondity = shipment.Commodities.AddNew();
			var code = commondity.C4Codes.AddNew();
			code.CY_Data = "A";
			return code;
		}
	}
}
