using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoCusClassPartPivotPartialTest : TestCaseWithFactory
	{
		public void TestShowManufacturerIDInAddressList()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, pivot.CD_OA_Manufacturer_ZAddress);
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, pivot.CD_OA_Exporter_ZAddress);
		}
	}
}
