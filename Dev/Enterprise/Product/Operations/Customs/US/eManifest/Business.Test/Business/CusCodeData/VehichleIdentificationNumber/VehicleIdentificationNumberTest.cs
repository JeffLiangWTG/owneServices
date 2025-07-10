using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(VehicleIdentificationNumber))]
	sealed class VehicleIdentificationNumberTest : Customs.Business.Testing.CusCodeDataTest<VehicleIdentificationNumber>
	{
		public void TestSetDefaultValues()
		{
			var number = Factory.New<VehicleIdentificationNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.VehicleIdentificationNumber, number.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.VehicleIdentificationNumber, number.CY_Code);
		}

		public void TestParent()
		{
			var commodity = Factory.New<Commodity>();
			var number = Factory.New<VehicleIdentificationNumber>();
			number.CY_ParentID = commodity.PK;
			number.CY_ParentTableCode = commodity.TablePrefix;
			AssertEquals(commodity, number.Parent);
			number = commodity.VehicleIdentificationNumbers.AddNew();
			AssertEquals(commodity.TablePrefix, number.CY_ParentTableCode);
			AssertEquals(commodity.PK, number.CY_ParentID);
			AssertEquals(commodity, number.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var commondity = shipment.Commodities.AddNew();
			return commondity.VehicleIdentificationNumbers.AddNew();
		}
	}
}
