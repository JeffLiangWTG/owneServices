using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		public override void TestTotalIncludedCosts()
		{
			Assert("Not applicable for SG as it doesn't have all the charges", true);
		}

		public override void TestDiscountOrSurcharge()
		{
			Assert("Not applicable for SG as it doesn't have all the charges", true);
		}

		public override void TestTotalDutiableChargesNotIncludedInLinesCanBeSet()
		{
			Assert("Not applicable for SG as it doesn't have all the charges", true);
		}

		public override void TestIncoTermDescription()
		{
			InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
			Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
			Assert("Inco term should be not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		public void TestCalc_TNICurrency()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceHeaderInternal = GetNewInvoice();
			AssertEquals("Calc_TNICurrency", InvoiceHeaderInternal.Invoice_Currency.RX_Code, InvoiceHeaderWrapperInternal.Calc_TNICurrency.Code);
		}

		public override void TestCIFCurrency()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceHeaderInternal = GetNewInvoice();
			AssertEquals("CIFCurrency", InvoiceHeaderInternal.Invoice_Currency.RX_Code, InvoiceHeaderWrapperInternal.CIFCurrency.Code);
		}

		public override void TestFOBCurrency()
		{
			AssertEquals("Curency is empty as defaulting registry is set to false (in set-up)", null, InvoiceHeaderWrapperInternal.FOBCurrency);

			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceHeaderInternal = GetNewInvoice();
			AssertEquals("FOBCurrency", InvoiceHeaderInternal.Invoice_Currency.RX_Code, InvoiceHeaderWrapperInternal.FOBCurrency.Code);
		}

		public override void TestInvoiceCurr()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = ZString.Empty;
			base.TestInvoiceCurr();
		}

		#region Implementation
		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Singapore; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		protected override ZString JobMessageTypeCodeForExport
		{
			get { return MessageTypeCodeList.Codes.OUT; }
		}

		protected override ZString JobMessageTypeCodeForImport
		{
			get { return MessageTypeCodeList.Codes.IPT; }
		}

		#endregion
	}
}
