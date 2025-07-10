using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineGroupingRangeCollection))]
	sealed class InvoiceLineGroupingRangeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTotalNoOfSequences()
		{
			var invoiceLine = Invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.LineGroupingRanges;
			collection.RemoveAndDeleteAll();
			AssertEquals(0, collection.TotalNoOfSequences);
			collection.AddNew(2, 6);
			AssertEquals(5, collection.TotalNoOfSequences);
			collection.AddNew(7, 7);
			AssertEquals(6, collection.TotalNoOfSequences);
			invoiceLine.LineGroupingRanges.AddNew(1, 1);
			AssertEquals(7, collection.TotalNoOfSequences);
		}

		public void TestGetRangeWithLowestSequenceNo()
		{
			var invoiceLine = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.LineGroupingRanges.RemoveAndDeleteAll();
			invoiceLine.LineGroupingRanges.AddNew(2, 3);
			invoiceLine.LineGroupingRanges.AddNew(4, 4);
			var range3 = invoiceLine.LineGroupingRanges.AddNew(1, 1);
			invoiceLine.LineGroupingRanges.AddNew(5, 7);
			AssertEquals(range3, invoiceLine.LineGroupingRanges.GetRangeWithLowestSequenceNo());
		}

		public void TestAddNewWithParamenters()
		{
			var invoiceLine = Invoice.JobComInvoiceLines.AddNew();
			var range = invoiceLine.LineGroupingRanges.AddNew(1, 5);
			AssertEquals("StartSequenceNo", (ZShort)1, range.US_StartSequenceNo);
			AssertEquals("EndSequenceNo", (ZShort)5, range.US_EndSequenceNo);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineGroupingRangeCollection(Invoice.JobComInvoiceLines.AddNew());

		JobComInvoiceHeader invoice;
		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_IsInvoiceByRequest = true;
					invoice = declaration.Invoices.AddNew();
					invoice.US_IsLineGrouping = true;
				}

				return invoice;
			}
		}
	}
}
