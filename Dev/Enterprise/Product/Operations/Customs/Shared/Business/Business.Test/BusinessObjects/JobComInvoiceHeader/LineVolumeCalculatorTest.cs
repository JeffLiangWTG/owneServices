using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LineVolumeCalculatorTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorDoesNotAcceptNull()
		{
			new LineVolumeCalculator(null);
		}

		[ExpectNoExceptions]
		public void TestGetVolumeThrowsArgumentNullException()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			LineVolumeCalculator calculator = new LineVolumeCalculator(invoiceHeader);
			try
			{
				calculator.GetVolume(null);
				Fail("Did not throw exception");
			}
			catch (ArgumentNullException)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestGetVolumeThrowsExceptionWhenInvoiceHeaderDoesNotMatchConstructor()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			LineVolumeCalculator calculator = new LineVolumeCalculator(invoiceHeader2);
			try
			{
				calculator.GetVolume(invoiceLine);
				Fail("Did not throw exception");
			}
			catch (ApplicationException)
			{
			}
		}

		public void TestLineVolumeKnown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Volume = 500m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Volume = 300m;
			invoiceLine.JI_VolumeUQ = "M3";
			AssertEquals(new ZVolume(300m, "M3"), new LineVolumeCalculator(invoiceHeader).GetVolume(invoiceLine));
		}

		public void TestApportionVolumeByPriceWhenInvoiceVolumeKnown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Volume = 500m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Volume = 300m;
			invoiceLine1.JI_VolumeUQ = "M3";
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			LineVolumeCalculator calculator = new LineVolumeCalculator(invoiceHeader);
			ZVolume result = calculator.GetVolume(invoiceLine2);
			AssertEquals(new ZVolume(200m, "M3"), result);
		}

		public void TestCalculateLineWhenHeaderUnKnownAndLineUnknown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalVolume = 500m;
			declaration.JE_TotalVolumeUnit = "M3";
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 900m;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			LineVolumeCalculator calculator = new LineVolumeCalculator(invoiceHeader);
			ZVolume result = calculator.GetVolume(invoiceLine2);
			AssertEquals(new ZVolume(50m, "M3"), result);
		}
	}
}
