using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorDependOnPGAExpeditedReleaseIndicatorTest : TestCaseWithFactory
	{
		public void TestPGAExpeditedReleaseIndicate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PGAExpeditedRelease = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			invoiceLine.JI_Description = "TEST FOR ENTRYSUMMERY";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains("PGA message should be sent",
@"OI        TEST FOR ENTRYSUMMERY                                                 
PG01001EPAODS                                                                   ", message.EM_FormattedMessageText);
		}
	}
}
