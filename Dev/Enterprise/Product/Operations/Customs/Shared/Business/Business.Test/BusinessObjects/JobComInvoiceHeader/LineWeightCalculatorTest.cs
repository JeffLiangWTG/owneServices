using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LineWeightCalculatorTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorDoesNotAcceptNull()
		{
			new LineWeightCalculator(null);
		}

		[ExpectNoExceptions]
		public void TestGetWeightThrowsArgumentNullException()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			LineWeightCalculator calculator = new LineWeightCalculator(invoiceHeader);
			try
			{
				calculator.GetWeight(null);
				Fail("Did not throw exception");
			}
			catch (ArgumentNullException)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestGetWeightThrowsExceptionWhenInvoiceHeaderDoesNotMatchConstructor()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			LineWeightCalculator calculator = new LineWeightCalculator(invoiceHeader2);
			try
			{
				calculator.GetWeight(invoiceLine);
				Fail("Did not throw exception");
			}
			catch (ApplicationException)
			{
			}
		}

		public void TestLineWeightCalculationOverride()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_Weight = 600m;
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			var invoiceLineMock1 = Factory.NewMoq<BaseJobComInvoiceLine>();
			invoiceLineMock1.Protected().Setup<ZDecimal>("LinePriceForWeightApportionCalculationCore").Returns(new ZDecimal(100m));
			BaseJobComInvoiceLine invoiceLine1 = invoiceLineMock1.Object;
			invoiceLine1.JI_JZ = invoiceHeader.PK;
			invoiceHeader.JobComInvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_LinePrice = 50m;

			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50m;
			AssertEquals(new ZWeight(400m, Core.Constants.Weight.Kilograms), new LineWeightCalculator(invoiceHeader).GetWeight(invoiceLine1));
			AssertEquals(new ZWeight(200m, Core.Constants.Weight.Kilograms), new LineWeightCalculator(invoiceHeader).GetWeight(invoiceLine2));
		}

		public void TestLineWeightKnown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Weight = 500m;
			invoiceHeader.JZ_WeightUQ = "KG";
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = 300m;
			invoiceLine.JI_WeightUQ = "KG";
			AssertEquals(new ZWeight(300m, "KG"), new LineWeightCalculator(invoiceHeader).GetWeight(invoiceLine));
		}

		public void TestGetWeightThrowsOverflowException()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Weight = 0.123456;
			invoiceHeader.JZ_WeightUQ = "KG";
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = decimal.MaxValue;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_LinePrice = 120m;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			LineWeightCalculator calculator = new LineWeightCalculator(invoiceHeader);

			AssertNoExceptionThrown(() => calculator.GetWeight(invoiceLine2));
		}

		public void TestApportionWeightByPriceWhenInvoiceWeightKnown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Weight = 500m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 300m;
			invoiceLine1.JI_WeightUQ = "KG";
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			LineWeightCalculator calculator = new LineWeightCalculator(invoiceHeader);
			ZWeight result = calculator.GetWeight(invoiceLine2);
			AssertEquals(new ZWeight(200m, "KG"), result);
		}

		public void TestCalculateLineWhenHeaderUnKnownAndLineUnknown()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalWeight = 500m;
			declaration.JE_TotalWeightUnit = "KG";
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 900m;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			LineWeightCalculator calculator = new LineWeightCalculator(invoiceHeader);
			ZWeight result = calculator.GetWeight(invoiceLine2);
			AssertEquals(new ZWeight(50m, "KG"), result);
		}
	}
}
