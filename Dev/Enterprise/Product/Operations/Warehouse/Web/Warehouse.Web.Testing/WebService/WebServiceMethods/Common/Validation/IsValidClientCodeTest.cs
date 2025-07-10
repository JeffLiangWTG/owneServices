using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class IsValidClientCodeTest : WhsSecureServiceTestCase
	{
		#region TestIsValidClientCode

		public void TestIsValidClientCode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff);
			var response1 = webService1.IsValidClientCode("");
			AssertEquals("Please provide a valid client code.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var webService2 = GetNewWebService();
			SetupSecurityHeader(webService2, data.Whs1, staff);
			var response2 = webService2.IsValidClientCode("XXX");
			AssertEquals("Please provide a valid client code.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff);
			var response3 = webService3.IsValidClientCode(data.Org1.OH_Code);
			AssertNull(response3.ErrorMessage);
			AssertEquals(ErrorTypes.None, response3.Error);
		}

		#endregion
	}
}
