using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class CusEntryHeaderExportUnionMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<ExportUnionMessage>
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.EUT;

		protected override string ExpectedApplicationReference => "ULU-B00001037";

		protected override IMessageSender Sender => provider.Object;

		protected override TRBaseMessageGenerator<ExportUnionMessage> Generator => generator;

		protected override void SetupData()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();

			var cusEntryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "ULU-B00001037";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			var jobComInvoiceLine = jobComInvoiceHeader.InvoiceLines.AddNew();
			jobComInvoiceLine.JI_CL = cusEntryLine.PK;

			provider = new Mock<IDeclarationForTest>();
			provider.Setup(x => x.Parent).Returns(cusEntryHeader);
			provider.Setup(x => x.Messages).Returns(cusEntryHeader.Messages);
			provider.Setup(x => x.JobReference).Returns(cusEntryHeader.CH_BGMReference);
			generator = new CusEntryHeaderExportUnionMessageGenerator(provider.Object);
		}
		CusEntryHeaderExportUnionMessageGenerator generator;
		Mock<IDeclarationForTest> provider;
	}
}
