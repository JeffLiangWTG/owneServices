using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class CusEntryHeaderControlMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<TRImportExportMessage>
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.DKO;

		protected override string ExpectedApplicationReference => "ULU-B00001037";

		protected override IMessageSender Sender => provider.Object;

		protected override TRBaseMessageGenerator<TRImportExportMessage> Generator => generator;

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
			generator = new CusEntryHeaderControlMessageGenerator(provider.Object);
		}
		CusEntryHeaderControlMessageGenerator generator;
		Mock<IDeclarationForTest> provider;

		public void TestQuestionsAndAnswersShouldBeEmptyForDKO()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, "DKO");
				AssertNotNull(declaration.QuestionsAndAnswers);
				AssertEquals("DKO", 0, declaration.QuestionsAndAnswers.Count);

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, "DTE");
				AssertNotNull(declaration.QuestionsAndAnswers);
				AssertEquals("DTE", 3, declaration.QuestionsAndAnswers.Count);

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, "EUT");
				AssertNotNull(declaration.QuestionsAndAnswers);
				AssertEquals("EUT", 3, declaration.QuestionsAndAnswers.Count);
			}
		}

		public void TestGenerateDKOMessageShouldNotIncludeQuestionsAndAnswers()
		{
			var message = (TRBaseMessage)((ITRCustomsMessageGenerator)Generator).GenerateMessage();

			AssertNotContains("Sorular_cevaplar", message.EM_MessageText, true);
		}
	}

	public interface IDeclarationForTest : CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration.IDeclaration, IMessageSender
	{
	}
}
