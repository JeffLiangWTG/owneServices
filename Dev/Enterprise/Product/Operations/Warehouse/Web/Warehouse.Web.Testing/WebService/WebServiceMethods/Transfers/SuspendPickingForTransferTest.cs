using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SuspendIsPickingForTransferTest : WhsSecureServiceTestCase
	{
		public void TestSuspendPickingForTransfer_TransferCannotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var webService = GetNewWebService();
			var response = webService.SuspendPickingForTransfer(Guid.Empty);
			AssertEquals("Cannot find out Transfer.", response.ErrorMessage);
			AssertEquals("Validation error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestSuspendPickingForTransfer_TransferIsFinalized()
		{
			var helper = new WhsTestHelperFunctions(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // to commit inventory
			transfer.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(transfer);

			var webService = GetNewWebService();
			var response = webService.SuspendPickingForTransfer(transfer.PK.ToGuid());
			AssertEquals("Transfer is finalized, cannot set Is Picking.", response.ErrorMessage);
			AssertEquals("Validation error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestSuspendPickingForTransfer()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var someoneElse = Helper.CreateGlbStaff("S2", "S2");
			var helper = new WhsTestHelperFunctions(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 40m, locationA1, locationA2);
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 20m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // to commit inventory

			AssertEquals("Precondition: Transfer line has matching lines", true, transferLine1.MatchingLines.Any());
			AssertEquals("Precondition: Transfer line has no matching lines", false, transferLine2.MatchingLines.Any());

			transferLine1.GS_NKPickedBy = staff1.GS_Code;
			var pickLines = transferLine1.PickLines;
			var pickLinesInMatchingLines = transferLine1.MatchingLines.SelectMany(ml => ml.PickLines);
			foreach (var pickLine in pickLines.Concat(pickLinesInMatchingLines))
			{
				pickLine.WZ_IsPicking = true;
				pickLine.WZ_GS_NKAssignedTo = someoneElse.GS_Code;
			}

			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine2.GS_NKPickedBy = staff1.GS_Code;

			AssertEquals("Precondition: All pick lines are assigned.", true, pickLines.All(pl => pl.WZ_GS_NKAssignedTo == someoneElse.GS_Code));

			Helper.Factory.Save();

			AssertEquals("Precondition: Transfer is not Finalised", false, transfer.IsFinalised);
			AssertEquals("Precondition: Transfer Line1 has pick line that is picking", true, transferLine1.IsPickerPicking);
			AssertEquals("Precondition: Transfer Line2 is picked", true, transferLine2.IsPicked);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.SuspendPickingForTransfer(transfer.PK.ToGuid());
			AssertNull("Should has no error", response.ErrorMessage);
			AssertEquals("Validation error.", ErrorTypes.None, response.Error);

			var factoryNew = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferLine1InNewFactory = factoryNew.Load<WhsTransferLine>(transferLine1.PK);
			AssertEquals("All pick lines in transfer line should not being picking", false, transferLine1InNewFactory.PickLines.All(pl => pl.WZ_IsPicking));
			AssertEquals("All pick lines in matching line should not being picking", false, transferLine1InNewFactory.MatchingLines.All(ml => ml.PickLines.All(pl => pl.WZ_IsPicking)));
			AssertEquals("All pick lines are unassigned.", true, transferLine1InNewFactory.PickLines.All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty));
			AssertEquals("All matching are unassigned.", true, transferLine1InNewFactory.MatchingLines.All(ml => ml.PickLines.All(pl => pl.WZ_GS_NKAssignedTo.IsEmpty)));
			var transferLine2InNewFactory = factoryNew.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("All pick lines in transfer line should not being picking", false, transferLine2InNewFactory.PickLines.All(pl => pl.WZ_IsPicking));
		}

		#region TestSuspendPickingForTransferClearPutawayBy

		public void TestSuspendPickingForTransferClearPutawayBy()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var helper = new WhsTestHelperFunctions(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 40m, locationA1, locationA2);
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 20m, locationA1, locationA2);
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part2, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // to commit inventory

			AssertEquals("Precondition: Transfer line has matching lines", true, transferLine1.MatchingLines.Any());
			AssertEquals("Precondition: Transfer line has no matching lines", false, transferLine2.MatchingLines.Any());
			AssertEquals("Precondition: Transfer line has no matching lines", false, transferLine3.MatchingLines.Any());

			transferLine1.GS_NKPickedBy = staff1.GS_Code;
			transferLine2.GS_NKPickedBy = staff1.GS_Code;
			transferLine3.GS_NKPickedBy = staff1.GS_Code;
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine3.WE_GS_NKPutawayBy = staff1.GS_Code;
			foreach (var transferLine in transfer.Lines)
			{
				transferLine.PickLines.Single().WZ_IsPicking = true;
			}
			var pickLinesInMatchingLines = transferLine1.MatchingLines.Single().PickLines.Single().WZ_IsPicking = true;
			AssertEquals("Precondition", true, transfer.Lines.SelectMany(l => l.PickLines).All(pl => pl.WZ_GS_NKAssignedTo == staff1.GS_Code));
			AssertEquals("Precondition", staff1.GS_Code, transferLine1.MatchingLines.Single().PickLines.Single().WZ_GS_NKAssignedTo);

			AssertEquals("Precondition: All pick lines are assigned.", true, transfer.Lines.All(l => l.WE_GS_NKPutawayBy == staff1.GS_Code));
			AssertEquals("Precondition: All pick lines are assigned.", true, transfer.Lines.SelectMany(l => l.PickLines).All(pl => pl.WZ_GS_NKAssignedTo == staff1.GS_Code));

			transferLine2.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			AssertEquals("Precondition: Transfer is not Finalised", false, transfer.IsFinalised);
			AssertEquals("Precondition: Transfer Line1 has pick line that is picking", true, transferLine1.MatchingLines.Single().PickLines.Single().WZ_IsPicking);
			AssertEquals("Precondition: Transfer Line1 is picked", false, transferLine1.IsPicked);
			AssertEquals("Precondition: Transfer Line2 is picked", true, transferLine2.IsPicked);
			AssertEquals("Precondition: Transfer Line3 is picked", false, transferLine3.IsPicked);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.SuspendPickingForTransfer(transfer.PK.ToGuid());
			AssertNull("Should has no error", response.ErrorMessage);
			AssertEquals("Validation error.", ErrorTypes.None, response.Error);

			var factoryNew = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInNewFactory = factoryNew.Load<WhsTransfer>(transfer.PK);
			AssertEquals("All pick lines in transfer line should not being picking.", false, transferInNewFactory.Lines.Any(l => l.PickLines.Any(pl => pl.WZ_IsPicking)));
			AssertEquals("transfer line 1 is picked and should be clear Putaway By.", true, transferInNewFactory.Lines.Single(l => l.WE_TransactionQuantity == 30).WE_GS_NKPutawayBy.IsEmpty); // 40 - 10
			AssertEquals("transfer line 1 is picked and should be clear Putaway By.", true, transferInNewFactory.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransactionQuantity == 30).MatchingLines.Single().WE_GS_NKPutawayBy.IsEmpty); // 10
			AssertEquals("transfer line 2 is picked and should not clear Putaway By.", false, transferInNewFactory.Lines.Single(l => l.WE_TransactionQuantity == 20).WE_GS_NKPutawayBy.IsEmpty);
			AssertEquals("transfer line 3 is picked and should not clear Putaway By.", true, transferInNewFactory.Lines.Single(l => l.WE_TransactionQuantity == 10).WE_GS_NKPutawayBy.IsEmpty);
		}

		#endregion

	}
}
