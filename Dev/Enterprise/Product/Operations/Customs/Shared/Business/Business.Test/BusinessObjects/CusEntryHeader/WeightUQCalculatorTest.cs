using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class WeightUQCalculatorTest : TestCaseWithFactory
	{
		public void TestDefaultUQ()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			WeightUQCalculator calculator = new WeightUQCalculator(entryHeader);
			AssertEquals("KG", calculator.UQ);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestEmptyParameterThrowsException()
		{
			new WeightUQCalculator(null);
		}

		public void TestDefaultWeight()
		{
			var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
			CusEntryHeader entryHeader = entryHeaderMock.Object;
			WeightUQCalculator calculator = new WeightUQCalculator(entryHeader);
			AssertEquals(0m, calculator.Weight);
		}

		public void TestWeightKeepsBestUnit()
		{
			var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)Factory.New(JobComInvoiceHeaderType);
			invoiceHeader.JZ_Weight = 500;
			invoiceHeader.JZ_WeightUQ = "LB";
			ArrayList x = new ArrayList { invoiceHeader };
			entryHeaderMock
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])x.ToArray(invoiceHeader.GetType()));
			CusEntryHeader entryHeader = entryHeaderMock.Object;
			WeightUQCalculator calculator = new WeightUQCalculator(entryHeader);
			AssertEquals("LB", calculator.UQ);
			AssertEquals(500m, calculator.Weight);
		}

		public void TestWeightAsUnit()
		{
			var mock = new Mock<WeightUQCalculator>();
			WeightUQCalculator calculator = mock.Object;
			mock.Setup(m => m.Weight).Returns(new ZDecimal(10));
			mock.Setup(m => m.UQ).Returns("T");
			AssertEquals(10000m, calculator.WeightAsUnit("KG"));
		}

		public void TestPreferredWeightEmptyElementsInInvoiceHeaders()
		{
			var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)Factory.New(JobComInvoiceHeaderType);
			invoiceHeader.JZ_Weight = 500;
			invoiceHeader.JZ_WeightUQ = "LB";
			ArrayList x = new ArrayList { invoiceHeader };
			entryHeaderMock
				.Protected()
				.Setup<BaseJobComInvoiceHeader[]>("GetInvoiceHeaders")
				.Returns((BaseJobComInvoiceHeader[])x.ToArray(invoiceHeader.GetType()));
			CusEntryHeader entryHeader = entryHeaderMock.Object;

			entryHeader.InvoiceHeaders[0].Delete();
			WeightUQCalculator calculator = new WeightUQCalculator(entryHeader);

			var invoice = calculator.UQ;
			AssertEquals("Because the element is deleted, invoice should equal 'KG'.", invoice, "KG");
		}

		protected virtual Type CusEntryHeaderType
		{
			get { return typeof(CusEntryHeader); }
		}

		protected virtual Type JobComInvoiceHeaderType
		{
			get { return typeof(BaseJobComInvoiceHeader); }
		}
	}
}
