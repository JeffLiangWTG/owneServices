using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		public override void TestOriginCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.CostaRica;

			var wrapper = CreateInvoiceLineWrapper(invoiceLine);
			AssertEquals("Invoice Line Country Of Origin", Core.Constants.CountryCodes.CostaRica, wrapper.OriginCode);

			invoiceLine.US_UC_NKCountryOfOrigin = "";
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.CostaRica;
			AssertEquals("Invoice Header Country Of Origin", Core.Constants.CountryCodes.CostaRica, wrapper.OriginCode);

			invoice.US_UC_NKCountryOfOrigin = "";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			AssertEquals("Invoice Line Country Of Origin", Core.Constants.CountryCodes.France, wrapper.OriginCode);

			invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.CostaRica;

			wrapper = CreateInvoiceLineWrapper(invoiceLine);
			AssertEquals("Invoice Line Country Of Origin", Core.Constants.CountryCodes.CostaRica, wrapper.OriginCode);
		}

		public override void TestMergedLineNo()
		{
			EntryLine.CL_LineNumber = 5;
			AssertEquals(InvoiceLineWrapper.MergedLineNo, "005");
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
