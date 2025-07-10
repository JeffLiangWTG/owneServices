using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsAdHocServiceJobValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckWSJ_CustomerReference_IsUniquePerClient

		public void TestCheckWSJ_CustomerReference_IsUniquePerClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			adhocServiceJob.WSJ_CustomerReference = "ABC";

			Factory.Save();

			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob2.WSJ_CustomerReferenceInfo);

			adhocServiceJob2.WSJ_CustomerReference = "XYZ";
			AssertNoErrors(adhocServiceJob2.WSJ_CustomerReferenceInfo);

			adhocServiceJob2.WSJ_CustomerReference = "ABC";
			AssertHasError(adhocServiceJob2.WSJ_CustomerReferenceInfo, "Customer Reference must be unique per Client.");

			var client2 = Helper.CreateClient();
			adhocServiceJob2.WSJ_OH_Client = client2.PK;
			adhocServiceJob2.WSJ_CustomerReference = "ABC";
			AssertEquals("Precondition", "ABC", adhocServiceJob2.WSJ_CustomerReference);
			AssertNoErrors("Change to different client, remove the error.", adhocServiceJob2.WSJ_CustomerReferenceInfo);

			adhocServiceJob2.Delete();
		}

		#endregion

		#region TestCheckWSJ_CustomerReferenceIsNotEmpty

		public void TestCheckWSJ_CustomerReferenceIsNotEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob.WSJ_CustomerReferenceInfo);

			AssertNoErrors("Should not have an Error if the Job is not in the Database.", adhocServiceJob.WSJ_CustomerReferenceInfo);
			Factory.Save();
			AssertEquals("Precondition: Customer Reference is populated on Save.", false, adhocServiceJob.WSJ_CustomerReference.IsEmpty);
			AssertNoErrors("Precondition", adhocServiceJob.WSJ_CustomerReferenceInfo);

			adhocServiceJob.WSJ_CustomerReference = "";
			AssertHasError(adhocServiceJob.WSJ_CustomerReferenceInfo, "Please enter a Customer Reference Number.");
		}

		#endregion

		#region TestCheckBillingDate

		public void TestCheckBillingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob.BillingDateInfo);

			adhocServiceJob.BillingDate = ZDateTime.Empty;
			AssertHasError(adhocServiceJob.BillingDateInfo, "Please enter a Billing Date.");

			adhocServiceJob.BillingDate = ZDateTime.Today;
			AssertNoErrors("BillingDate is set, should have no errors.", adhocServiceJob.BillingDateInfo);

			adhocServiceJob.Delete();
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var existingJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			existingJob.WSJ_CustomerReference = "ABC";
			Factory.Save();

			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertNoErrors(adhocServiceJob.WSJ_OH_ClientInfo);
			AssertNoErrors(adhocServiceJob.WSJ_WW_WhsInfo);
			AssertNoErrors(adhocServiceJob.BillingDateInfo);
			AssertNoErrors(adhocServiceJob.WSJ_CustomerReferenceInfo);

			adhocServiceJob.BillingDate = ZDateTime.Empty;
			adhocServiceJob.Validation.ValidateAll();

			AssertHasError(adhocServiceJob.WSJ_OH_ClientInfo, "Please enter a Client.");
			AssertHasError(adhocServiceJob.WSJ_WW_WhsInfo, "Please enter a Warehouse.");
			AssertHasError(adhocServiceJob.BillingDateInfo, "Please enter a Billing Date.");
			AssertNoErrors(adhocServiceJob.WSJ_CustomerReferenceInfo);

			adhocServiceJob.WSJ_OH_Client = data.Org1.PK;
			adhocServiceJob.WSJ_WW_Whs = data.Whs1.PK;
			adhocServiceJob.BillingDate = ZDateTime.Today;
			adhocServiceJob.WSJ_CustomerReference = "ABC";
			adhocServiceJob.Validation.ValidateAll();

			AssertNoErrors(adhocServiceJob.WSJ_OH_ClientInfo);
			AssertNoErrors(adhocServiceJob.WSJ_WW_WhsInfo);
			AssertNoErrors(adhocServiceJob.BillingDateInfo);
			AssertHasError(adhocServiceJob.WSJ_CustomerReferenceInfo, "Customer Reference must be unique per Client.");

			adhocServiceJob.WSJ_CustomerReference = "XYZ";
			adhocServiceJob.Validation.ValidateAll();
			AssertNoErrors(adhocServiceJob.WSJ_CustomerReferenceInfo);
		}

		#endregion

		#region TestValidateWarehouse

		#region TestValidateWarehouse_IsWarehouseValid

		public void TestValidateWarehouse_IsWarehouseValid()
		{
			var client = Helper.CreateClient("Client");
			var whs1 = Helper.CreateWarehouse("Whs1");
			whs1.WW_IsVirtualWarehouse = false;
			whs1.WW_IsActive = true;
			whs1.WW_WarehouseType = Integration.CodeLists.WarehouseTypes.Codes.Product;

			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs1, client, ZDateTime.Today);
			adhocServiceJob.WSJ_WW_Whs = ZGuid.Invalid;
			AssertHasError(adhocServiceJob.WSJ_WW_WhsInfo, "Enter a valid Warehouse.");

			adhocServiceJob.Delete();
		}

		#endregion

		#region TestValidateWarehouse_ActiveWarehouse

		public void TestValidateWarehouse_ActiveWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob.WSJ_WW_WhsInfo);

			adhocServiceJob.WSJ_WW_Whs = data.Whs1.PK;
			AssertNoErrors("WSJ_WW_Whs is set, no errors.", adhocServiceJob.WSJ_WW_WhsInfo);

			adhocServiceJob.WSJ_WW_Whs = ZGuid.Empty;
			AssertHasError(adhocServiceJob.WSJ_WW_WhsInfo, "Please enter a Warehouse.");

			adhocServiceJob.WSJ_WW_Whs = data.Whs1.PK;
			AssertNoErrors("WSJ_WW_Whs is set, no errors.", adhocServiceJob.WSJ_WW_WhsInfo);

			var warehouse2 = Helper.CreateWarehouse("Whs2");
			warehouse2.WW_IsActive = false;
			adhocServiceJob.WSJ_WW_Whs = warehouse2.PK;
			AssertHasError(adhocServiceJob.WSJ_WW_WhsInfo, "This Warehouse is inactive - it may not be used.");

			adhocServiceJob.WSJ_WW_Whs = data.Whs1.PK;
			AssertNoErrors("WSJ_WW_Whs is set, no errors.", adhocServiceJob.WSJ_WW_WhsInfo);

			adhocServiceJob.Delete();
		}

		#endregion

		#endregion

		#region TestValidateClient

		public void TestValidateClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob.WSJ_OH_ClientInfo);

			adhocServiceJob.WSJ_OH_Client = ZGuid.Invalid;
			AssertHasError(adhocServiceJob.WSJ_OH_ClientInfo, "Enter a valid Client.");

			adhocServiceJob.WSJ_OH_Client = ZGuid.Empty;
			AssertHasError(adhocServiceJob.WSJ_OH_ClientInfo, "Please enter a Client.");

			adhocServiceJob.WSJ_OH_Client = data.Org1.PK;
			AssertNoErrors("WSJ_OH_Client is set, no errors.", adhocServiceJob.WSJ_OH_ClientInfo);

			adhocServiceJob.Delete();
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
