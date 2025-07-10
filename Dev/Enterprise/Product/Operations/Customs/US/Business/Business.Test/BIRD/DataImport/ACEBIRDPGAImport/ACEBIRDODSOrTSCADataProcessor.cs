using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class ACEBIRDODSOrTSCADataProcessor : ACEBIRDCommonPGADataProcessorTest
	{
		protected new ISEAdditionalData GetAction(JobDeclaration declaration)
		{
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			return ACEEntrySummaryMessageSendingOption.New(action);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			invoiceLine.US_FDAContactName = "IAN TEST BROKER";
			invoiceLine.US_FDAContactPhoneNo = "164285648734";
			invoiceLine.US_FDAContactEmail = "ABCDEFG@TEST.COM";
		}
	}
}
