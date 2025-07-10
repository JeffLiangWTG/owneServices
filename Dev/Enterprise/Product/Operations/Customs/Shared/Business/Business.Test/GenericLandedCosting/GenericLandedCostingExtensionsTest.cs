using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GenericLandedCostingExtensionsTest : TestCaseWithFactory
	{
		public void TestGetAmountApportionedFromCusEntryHeader()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine1 = Factory.New<CusEntryLineForTesting>();
			entryLine1.XX_Applicable = true;
			entryHeader.MergedLines.Add(entryLine1);
			AssertEquals(100m, entryLine1.GetAmountApportionedFromCusEntryHeader(100m, "XX_Applicable"));

			var entryLine2 = Factory.New<CusEntryLineForTesting>();
			var entryLine3 = Factory.New<CusEntryLineForTesting>();
			entryHeader.MergedLines.Add(entryLine2);
			entryHeader.MergedLines.Add(entryLine3);

			entryLine1.CL_CustomsValue = 20m;
			entryLine2.CL_CustomsValue = 30m;
			entryLine2.XX_Applicable = true;
			entryLine3.CL_CustomsValue = 50m;
			entryLine3.XX_Applicable = false;

			AssertEquals(20m, entryLine1.GetAmountApportionedFromCusEntryHeader(100m, ""));
			AssertEquals(30m, entryLine2.GetAmountApportionedFromCusEntryHeader(100m, ""));
			AssertEquals(50m, entryLine3.GetAmountApportionedFromCusEntryHeader(100m, ""));

			AssertEquals(40m, entryLine1.GetAmountApportionedFromCusEntryHeader(100m, "XX_Applicable"));
			AssertEquals(60m, entryLine2.GetAmountApportionedFromCusEntryHeader(100m, "XX_Applicable"));
			AssertEquals(0m, entryLine3.GetAmountApportionedFromCusEntryHeader(100m, "XX_Applicable"));
		}

		public void TestGetAmountApportionedFromCusEntryLine()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Env.CurrentCompany.LocalCurrency.Code;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invoiceLine1);

			AssertEquals(100m, invoiceLine1.GetAmountApportionedFromCusEntryLine(entryLine, 100m));

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine2);
			entryLine.InvoiceLines.Add(invoiceLine3);

			invoiceLine1.JI_LinePrice = 20m;
			invoiceLine2.JI_LinePrice = 30m;
			invoiceLine3.JI_LinePrice = 50m;

			AssertEquals(20m, invoiceLine1.GetAmountApportionedFromCusEntryLine(entryLine, 100m));
			AssertEquals(30m, invoiceLine2.GetAmountApportionedFromCusEntryLine(entryLine, 100m));
			AssertEquals(50m, invoiceLine3.GetAmountApportionedFromCusEntryLine(entryLine, 100m));
		}

		public void TestGetPropertyValue()
		{
			var entryLine = Factory.New<CusEntryLineForTesting>();

			AssertExceptionThrown<ArgumentException>("Should throw an exception", () => entryLine.GetPropertyValue<ZBool>("XXXX"));
			AssertExceptionThrown<ArgumentException>("Should throw an exception", () => entryLine.GetPropertyValue<ZString>("XX_Applicable"));
			Assert(!entryLine.GetPropertyValue<ZBool>("XX_Applicable"));
		}

		class CusEntryLineForTesting : CusEntryLine
		{
			public CusEntryLineForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZBool XX_Applicable { get; set; }
		}
	}
}
