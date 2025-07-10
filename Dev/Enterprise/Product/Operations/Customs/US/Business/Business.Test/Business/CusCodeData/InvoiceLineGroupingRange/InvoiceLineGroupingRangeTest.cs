using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineGroupingRange))]
	sealed class InvoiceLineGroupingRangeTest : Customs.Business.Testing.CusCodeDataTest<InvoiceLineGroupingRange>
	{
		public void TestNoOfSequences()
		{
			var numberRange = Factory.New<InvoiceLineGroupingRange>();
			numberRange.US_StartSequenceNo = 1;
			numberRange.US_EndSequenceNo = 3;
			AssertEquals(3, numberRange.NoOfSequences);
			numberRange.US_EndSequenceNo = 1;
			AssertEquals(1, numberRange.NoOfSequences);
			numberRange.US_EndSequenceNo = -1;
			AssertEquals(0, numberRange.NoOfSequences);
			numberRange.US_EndSequenceNo = 0;
			AssertEquals(0, numberRange.NoOfSequences);
			numberRange.US_EndSequenceNo = 3;
			numberRange.US_StartSequenceNo = -1;
			AssertEquals(0, numberRange.NoOfSequences);
			numberRange.US_StartSequenceNo = 0;
			AssertEquals(0, numberRange.NoOfSequences);
		}

		public void TestDefaultValues()
		{
			var numberRange = Factory.New<InvoiceLineGroupingRange>();
			AssertEquals(CusCodeDataTypeList.Codes.InvoiceLineNumberRange, numberRange.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.InvoiceLineNumberRange, numberRange.CY_Code);
		}

		public void TestStartSequenceNoAndEndNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var numberRange = Factory.New<InvoiceLineGroupingRange>();
			numberRange.US_StartSequenceNo = 4;
			numberRange.US_EndSequenceNo = 10;
			numberRange.Parent = invoiceLine;
			Factory.Save();
			AssertEquals("4:10", numberRange.CY_Data);
			var newFactory = new BusinessObjectFactory();
			var reloadedRange = newFactory.Load<InvoiceLineGroupingRange>(numberRange.PK);
			AssertEquals(invoiceLine.PK, reloadedRange.Parent.PK);
			AssertEquals("4:10", reloadedRange.CY_Data);
			AssertEquals((ZShort)4, reloadedRange.US_StartSequenceNo);
			AssertEquals((ZShort)10, reloadedRange.US_EndSequenceNo);
			reloadedRange.CY_Data = "1:5";
			AssertEquals((ZShort)1, reloadedRange.US_StartSequenceNo);
			AssertEquals((ZShort)5, reloadedRange.US_EndSequenceNo);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_IsInvoiceByRequest = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.LineGroupingRanges.AddNew();
		}
	}
}
