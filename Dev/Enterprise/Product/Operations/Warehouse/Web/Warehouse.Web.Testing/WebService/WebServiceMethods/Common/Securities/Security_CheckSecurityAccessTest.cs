using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Security_CheckSecurityAccessTest : WhsSecureServiceTestCase
	{
		#region TestSecurity_CheckSecurityAccess

		public void TestSecurity_CheckSecurityAccess_Unload()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Unload, Env.Security.WhsRFScanningUnloadEdit);
		}

		public void TestSecurity_CheckSecurityAccess_UnloadDuplicatePreviousLine()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.UnloadDuplicatePreviousLine, Env.Security.WhsRFScanningUnloadDuplicatePreviousLine);
		}

		public void TestSecurity_CheckSecurityAccess_Putaway()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Putaway, Env.Security.WhsRFScanningPutawayEdit);
		}

		public void TestSecurity_CheckSecurityAccess_ReceiveFinalise()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.ReceiveFinalise, Env.Security.WhsReceiveFinalise);
		}

		public void TestSecurity_CheckSecurityAccess_Picking()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Picking, Env.Security.WhsRFScanningPickingEdit);
		}

		public void TestSecurity_CheckSecurityAccess_Inventory()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Inventory, Env.Security.WhsRFScanningInventoryView);
		}

		public void TestSecurity_CheckSecurityAccess_Stocktake()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Stocktake, Env.Security.WhsRFScanningStocktakeEdit);
		}

		public void TestSecurity_CheckSecurityAccess_Release()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Release, Env.Security.WhsRFScanningReleaseEdit);
		}

		public void TestSecurity_CheckSecurityAccess_Transfers()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.Transfers, Env.Security.WhsRFScanningTransfersEdit);
		}

		public void TestSecurity_CheckSecurityAccess_AllowBadScan()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.AllowBadScan, Env.Security.WhsRFScanningAllowBadScan);
		}

		public void TestSecurity_CheckSecurityAccess_BadScan()
		{
			TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.AuthorizeBadScan, Env.Security.WhsRFScanningAuthorizeBadScan);
		}

		public void TestSecurity_CheckSecurityAccess_InvalidSecurityAccessType()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse, staff);

			AssertBusinessValidationError(webService, "Invalid Security Access Type!", webService.Security_CheckSecurityAccess((WhsSecureService.RFSecurityAccessType)999));
		}

		void TestSecurity_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType securityAccessType, SecurityCheckpoint securityCheckpoint)
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse, staff);

			securityCheckpoint.IsAllowed = true;

			var response = webService1.Security_CheckSecurityAccess(securityAccessType);
			AssertSuccessfulResponse(response, webService1);
			CombineAssertions(() =>
			{
				AssertEquals(true, response.HasAccess);
				AssertEquals("", response.Message);
			});

			var webService2 = GetNewWebService(warehouse, staff);

			securityCheckpoint.IsAllowed = false;

			var response2 = webService2.Security_CheckSecurityAccess(securityAccessType);
			AssertSuccessfulResponse(response2, webService2);
			CombineAssertions(() =>
			{
				AssertEquals(false, response2.HasAccess);
				AssertEquals(securityCheckpoint.ErrorMessageForNotAllowed, response2.Message);
			});
		}

		#endregion

		public void TestSecurity_CheckSecurityAccess_DoesNotCreateUserSemaphoreIfItDoesNotRequireConcurencyLoginValidation()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var staff2 = Helper.CreateGlbStaff("ST2", "ST2");
			Helper.Factory.Save();

			AssertEquals("Precondition - no active logins expected.", 0, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
			var webService1 = GetNewWebService(warehouse, staff1);
			Env.Security.WhsRFScanningAuthorizeBadScan.IsAllowed = true;

			var response = webService1.Security_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.AuthorizeBadScan);
			AssertSuccessfulResponse(response, webService1);
			CombineAssertions(() =>
			{
				AssertEquals(true, response.HasAccess);
				AssertEquals("", response.Message);
			});

			AssertEquals("Precondition - 1 active login expected.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
			var webService2 = GetNewWebService(warehouse, staff2);
			Env.Security.WhsRFScanningAuthorizeBadScan.IsAllowed = true;

			var response2 = webService2.Security_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.AuthorizeBadScan, false);
			AssertSuccessfulResponse(response2, webService2);
			CombineAssertions(() =>
			{
				AssertEquals(true, response.HasAccess);
				AssertEquals("", response.Message);
			});
			AssertEquals("Only 1 active login expected.", 1, LoginHelper.ActiveSemaphoreHandlersForTesting.Count);
		}

		public void TestSecurity_CheckSecurityAccess_FailsValidateWebServiceAndStaffAndWarehouse()
		{
			var webService = GetNewWebService();
			var response = webService.Security_CheckSecurityAccess(WhsSecureService.RFSecurityAccessType.AuthorizeBadScan, false);

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals(false, response.HasAccess);
				AssertEquals("Please provide login credentials to use this service.", response.ErrorMessage);
			});
		}
	}
}
