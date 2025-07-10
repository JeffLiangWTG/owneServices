using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class TypeSafeCusEntryHeaderTest : TestCaseWithFactory
	{
		public void TestDeclarationTypeIsJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<JobDeclaration>(entryHeader.Declaration);
		}

		public void TestLookupsIsNotNull()
		{
			AssertNotNull(Factory.New<CusEntryHeader>().Lookups);
		}

		public void TestRandomHeaderIsJobComInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			line.JI_CL = entryLine.PK;
			entryLine.CL_CH = entryHeader.PK;
			entryHeader.MergedLines.Add(entryLine);
			AssertType<JobComInvoiceHeader>(entryHeader.RandomHeader);
		}

		public void TestInvoiceHeadersIsArrayOfJobComInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			line.JI_CL = entryLine.PK;
			entryLine.CL_CH = entryHeader.PK;
			entryHeader.MergedLines.Add(entryLine);
			AssertType<JobComInvoiceHeader[]>(entryHeader.InvoiceHeaders);
		}

		public void TestMergedLinesIsCusEntryLineCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			line.JI_CL = entryLine.PK;
			entryLine.CL_CH = entryHeader.PK;
			entryHeader.MergedLines.Add(entryLine);
			AssertType<Customs.Business.CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
		}

		public void TestWeightCalculator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<WeightUQCalculator>(entryHeader.WeightCalculator);
		}

		public void TestMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.AddNew();
			AssertType<EDIMessageCollection>(entryHeader.Messages);
		}

		public void TestEntryPayInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryPayInfos.AddNew();
			AssertType<Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>>(entryHeader.EntryPayInfos);
		}
	}
}
