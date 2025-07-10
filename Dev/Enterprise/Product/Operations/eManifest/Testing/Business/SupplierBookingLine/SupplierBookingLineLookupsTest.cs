using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eManifest.Business.Testing
{
	internal class SupplierBookingLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStateLists()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";

			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_RN_NKCountryCode = country.RN_Code;

			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			AssertEquals("Consignee State List should have no elements.", 0, line.Lookups.ConsigneeStateList.Count);
			AssertEquals("Consignor State List should have no elements.", 0, line.Lookups.ConsignorStateList.Count);

			line.DL_RN_NKConsigneeCountryCode = "X7";
			AssertEquals("Consignee State List should have 2 elements.", 2, line.Lookups.ConsigneeStateList.Count);
			AssertEquals("Consignor State List should have no elements.", 0, line.Lookups.ConsignorStateList.Count);

			line.DL_RN_NKConsignorCountryCode = "X7";
			AssertEquals("Consignee State List should have 2 elements.", 2, line.Lookups.ConsigneeStateList.Count);
			AssertEquals("Consignor State List should have 2 elements.", 2, line.Lookups.ConsignorStateList.Count);
		}
	}
}

