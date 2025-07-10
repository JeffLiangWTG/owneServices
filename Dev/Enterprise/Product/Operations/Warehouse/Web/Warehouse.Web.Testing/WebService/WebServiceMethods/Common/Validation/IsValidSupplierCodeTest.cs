using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class IsValidSupplierCodeTest : WhsSecureServiceTestCase
	{
		#region TestIsValidSupplierCode

		public void TestIsValidSupplierCode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.IsValidSupplierCode("");
			AssertEquals("Please provide a valid supplier code.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.IsValidSupplierCode("XXX");
			AssertEquals("Please provide a valid supplier code.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var webService3 = GetNewWebService(data.Whs1, staff);

			data.Org1.OH_IsConsignor = false;
			Helper.Factory.Save();

			var response3 = webService3.IsValidSupplierCode(data.Org1.OH_Code);
			AssertEquals("Please provide a valid supplier code.", response3.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);

			var webService4 = GetNewWebService(data.Whs1, staff);

			data.Org1.OH_IsConsignor = true;
			data.Org1.OH_IsActive = true;
			Helper.Factory.Save();

			var response4 = webService4.IsValidSupplierCode(data.Org1.OH_Code);
			AssertNull(response4.ErrorMessage);
			AssertEquals(ErrorTypes.None, response4.Error);
		}

		#endregion
	}
}
