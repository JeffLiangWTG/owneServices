using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.Testing
{
	class RemovePalletIDFromPutawayJobTest : WhsSecureServiceTestCase
	{
		public void TestRemovePalletIDFromPutawayJob_ValidJobWithLines_RemoveSuccess_UpperCase()
		{
			TestRemovePalletIDFromPutawayJob_ValidJobWithLines_RemoveSuccessCore("PLT-1");
		}

		public void TestRemovePalletIDFromPutawayJob_ValidJobWithLines_RemoveSuccess_LowerCase()
		{
			TestRemovePalletIDFromPutawayJob_ValidJobWithLines_RemoveSuccessCore("plt-1");
		}

		void TestRemovePalletIDFromPutawayJob_ValidJobWithLines_RemoveSuccessCore(string palletIDToRemove)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT3");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT-1", 50m);
			transferLine.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1a = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayLine1b = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);
			var putawayLine1c = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-3", false);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			AssertEquals("Correct unfinalised putaway line count", 3, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, data.Whs1, staff1).Count());
			var response = webService.RemovePalletIDFromPutawayJob(palletIDToRemove);
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				AssertEquals("Correct unfinalised putaway line count", 2, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), data.Whs1, staff1).Count());
				Assert("Correct job suspension", putawayLine1b.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine1c.WPL_IsPuttingAway);
				AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);
			});

			response = webService.RemovePalletIDFromPutawayJob(palletIDToRemove);
			AssertEquals($"Pallet {palletIDToRemove} cannot be found in current putaway job.", response.ErrorMessage);
		}

		public void TestRemovePalletIDFromPutawayJob_MultipleStaffWithJobsExist_RemoveSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
			var staff3 = Helper.CreateGlbStaff("S3", "Staff3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-2", 50m);
			transferLine.WE_GS_NKPutawayBy = staff2.GS_Code;
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob2, "PLT-2", true);
			var putawayJob3 = Helper.CreateWhsPutawayJob(data.Whs1, staff3);
			var putawayLine3 = Helper.CreateWhsPutawayLine(putawayJob3, "PLT-3", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff2);
			AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, data.Whs1, staff2).Count());
			var response = webService.RemovePalletIDFromPutawayJob("PLT-2");
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), data.Whs1, staff1).Count());
				AssertEquals("Correct unfinalised putaway line count", 0, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), data.Whs1, staff2).Count());
				AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), data.Whs1, staff3).Count());
				AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);
			});
		}

		public void TestRemovePalletIDFromPutawayJob_MultipleWhsWithJobsExist_RemoveSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			Helper.Factory.Save();

			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, whs2.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayJob2 = Helper.CreateWhsPutawayJob(whs2, staff1);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob2, "PLT-1", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, data.Whs1, staff1).Count());
			AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, whs2, staff1).Count());
			var response = webService.RemovePalletIDFromPutawayJob("PLT-1");
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				AssertEquals("Correct unfinalised putaway line count", 0, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), data.Whs1, staff1).Count());
				AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(new BusinessObjectFactory(), whs2, staff1).Count());
				AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);
			});
		}

		public void TestRemovePalletIDFromPutawayJob_NoStaffAndWarehouse_ReturnsError()
		{
			var webService = GetNewWebService();
			var response = webService.RemovePalletIDFromPutawayJob("PLT-1");
			AssertEquals("Please provide login credentials to use this service.", response.ErrorMessage);
		}

		public void TestRemovePalletIDFromPutawayJob_PalletDoesNotExist_ReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, data.Whs1, staff1).Count());
			var response = webService.RemovePalletIDFromPutawayJob("PLT-2");
			AssertEquals("Pallet PLT-2 cannot be found in current putaway job.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestRemovePalletIDFromPutawayJob_PalletDoesNotExistInJob_ReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			AssertEquals("Correct unfinalised putaway line count", 1, PutawayHelper.LoadUnfinalisedPutawayLines(webService.Factory, data.Whs1, staff1).Count());
			var response = webService.RemovePalletIDFromPutawayJob("PLT-2");
			AssertEquals("Pallet PLT-2 cannot be found in current putaway job.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestRemovePalletIDFromPutawayJob_NoJob_ReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.RemovePalletIDFromPutawayJob("PLT-1");
			AssertEquals("Putaway Job cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestRemovePalletIDFromPutawayJob_FinalisedJob_ReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isFinalised: true);
			putawayJob1.WPJ_FinalizedTimeUtc = DateTime.Now;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.RemovePalletIDFromPutawayJob("PLT-1");
			AssertEquals("Putaway Job cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestRemovePalletIDFromPutawayJob_PutawayTransferLineNotSetToUser_ReturnsError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1a = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.RemovePalletIDFromPutawayJob("PLT-1");
			AssertEquals("Putaway transfer line for pallet PLT-1 cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}
	}
}
