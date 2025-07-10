using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	abstract class ExportPGABlocksCreatorTest : TestCaseWithFactory
	{
		[TestDate(2016, 04, 22)]
		public void TestExportPGABlocks()
		{
			SetupData();

			var blocks = ExportPGABlocksCreator.BuildPGABlocks(entryLine);
			var messageBuilder = new ZStringBuilder();

			foreach (var block in blocks)
			{
				messageBuilder.AppendIfNotEmpty(block.Serialise());
			}

			AssertEquals(ExpectedResult, messageBuilder.ToStringWithNewLineBetweenAppends());
		}

		protected virtual void SetupData()
		{
			SetUp();
		}

		protected abstract ZString ExpectedResult { get; }

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
		}

		protected ExportPGABlocksCreator ExportPGABlocksCreator
		{
			get { return exportPGABlocksCreator ?? (exportPGABlocksCreator = new ExportPGABlocksCreator()); }
		}
		ExportPGABlocksCreator exportPGABlocksCreator;

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
	}
}
