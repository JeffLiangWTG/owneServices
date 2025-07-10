using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentConfirmationDocumentSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.DtbConsignConfirm, GetNewDocumentSupporter().BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbBookingConsignmentCustomiseDocuments, GetNewDocumentSupporter().CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers

		public void TestGetDocumentWrappers()
		{
			var supporter = GetNewDocumentSupporter();
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var wrappersForSomeGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertEquals("We print document for 1 confirmation, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);

			var wrapper = wrappersForSomeGenericFreightJobDocument[0];
			AssertEquals("The document wrapper should be the correct type.", "FreightWrapperFromDtbBookingConsignment", wrapper.GetType().Name);
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

		#region Implementation

		DtbConsignmentConfirmationDocumentSupporter GetNewDocumentSupporter()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var instruction = consignment.PickupInstruction;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp, ZDateTime.Now, ZDateTime.Now.AddDays(3));
			return new DtbConsignmentConfirmationDocumentSupporter(confirmation);
		}

		#endregion
	}
}
