using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		public override void TestLinePriceCurr()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceLine = GetNewInvoiceLine();
			AssertEquals("LinePriceCurr", InvoiceLine.InvoiceHeader.Invoice_Currency.RX_Code, InvoiceLineWrapper.LinePriceCurr.Code);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Singapore; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
