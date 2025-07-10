using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentConcurrencyCheckerTest : TestCaseWithFactory
	{
		public void TestDuplicateJobRequiredDocumentNotSavedIntoDatabase()
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobRequiredDocumentBTH = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Both;

			Factory.Save();

			var jobRequiredDocumentBTH2 = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			jobRequiredDocumentBTH2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			jobRequiredDocumentBTH2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			jobRequiredDocumentBTH2.EQ_DocUsage = Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Both;
			Factory.Save();

			AssertEquals("jobRequiredDocumentBTH2 should be deleted", true, jobRequiredDocumentBTH2.IsDeleted);
		}

		public void TestAllowTransactionWithOtherParticipants()
		{
			JobRequiredDocumentConcurrencyChecker.Register(Factory);

			AssertEquals(1, Factory.SaveInTransactionActions.Count);

			var concurrencyChecker = Factory.SaveInTransactionActions[0] as ITransactionParticipant;
			AssertEquals(true, concurrencyChecker.AllowTransactionWithOtherParticipant);
		}
	}
}
