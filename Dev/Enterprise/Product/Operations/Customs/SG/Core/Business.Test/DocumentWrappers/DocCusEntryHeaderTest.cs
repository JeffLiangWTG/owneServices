using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		public JobComInvoiceHeader GetInvoiceHeaderForEntryHeader()
		{
			var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			JobComInvoiceHeader invoiceHeader = EntryHeaderInternal.Declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = EntryHeaderInternal.MergedLines.AddNew().PK;
			return invoiceHeader;
		}

		public void TestNew()
		{
			AssertNull("Created with null", DocCusEntryHeader.New(null, Factory));
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertNotNull("Created with a valild object", DocCusEntryHeader.New(entryHeader, Factory));
		}

		public void TestDeclaration()
		{
			AssertNotNull("Declaration", EntryHeaderInternal.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), EntryHeaderWrapperInternal.Declaration.GetType());
		}

		public void TestEntryLinesCollection()
		{
			CusEntryLine line1 = EntryHeaderInternal.MergedLines.AddNew();
			CusEntryLine line2 = EntryHeaderInternal.MergedLines.AddNew();

			AssertEquals(2, EntryHeaderWrapperInternal.EntryLines.Count);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Singapore; }
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		#endregion
	}
}
