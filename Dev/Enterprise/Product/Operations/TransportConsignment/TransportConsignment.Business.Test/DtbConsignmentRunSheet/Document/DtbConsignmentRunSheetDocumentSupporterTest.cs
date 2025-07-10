using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetDocumentSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.DtbConsignRunSheet, GetNewDocumentSupporter().BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbConsignmentRunSheetCustomiseDocuments, GetNewDocumentSupporter().CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers()
		{
			var supporter = GetNewDocumentSupporter();
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var wrappersForSomeGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertEquals("We print document for 1 Booking, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);
		}

		#endregion

		#region TestSupportedDataContexts

		public void TestSupportedDataContexts()
		{
			AssertEquals(true, GetNewDocumentSupporter().IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.GenericFreightJob))));
		}

		#endregion

		#region TestShowReasonForNotPrinting

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", false, GetNewDocumentSupporter().ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		#endregion

		#region TestGetDataStateBeforeRun

		public void TestGetDataStateBeforeRunForCMRConsignmentNote()
		{
			var supporter = GetNewDocumentSupporter();
			var runSheet = supporter.BusinessObject as DtbConsignmentRunSheet;

			//run sheet does not contain any instructions
			AssertEquals("Precondition", 0, runSheet.RunSheetInstructions.Count);

			var commandAboutToBeRun = Factory.New<StmMenuItem>();
			commandAboutToBeRun.SU_MenuName = "CMR Consignment Note";
			var result = supporter.GetDataStateBeforeRun(commandAboutToBeRun);
			AssertEquals("result should be false when run sheet does not contain any consignments", false, result.IsValid);
			AssertEquals("error message regarding no consignment", "Run Sheet does not contain any Consignments.", result.ErrorMessage);

			var transportConsignmentHelper = new TransportConsignmentTestHelper(Factory);
			var consignment1 = transportConsignmentHelper.CreateConsignment();
			var address1 = transportConsignmentHelper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var action1 = address1.Actions.AddNew();
			AssertNull(action1.RunSheetInstruction);
			var runSheetInstruction1 = runSheet.RunSheetInstructions.AddNew();
			action1.LTA_K1_RunSheetInstruction = runSheetInstruction1.PK;
			AssertEquals(runSheetInstruction1, action1.RunSheetInstruction);

			result = supporter.GetDataStateBeforeRun(commandAboutToBeRun);
			AssertEquals("result should be true when run sheet contains consignments", true, result.IsValid);
			AssertEquals("error message should be empty", string.Empty, result.ErrorMessage);
		}

		#endregion

		#region Implementation

		DtbConsignmentRunSheetDocumentSupporter GetNewDocumentSupporter()
		{
			var runsheet = Helper.CreateRunSheet();
			return new DtbConsignmentRunSheetDocumentSupporter(runsheet);
		}

		#endregion
	}
}
