using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AIILineCollection))]
	public class AIILineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTotalInvoiceAmount()
		{
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			aiiLine1.US_InvAmount = 20m;
			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew(range);
			aiiLine2.US_InvAmount = 35m;
			AIILine aiiLine3 = InvoiceLine.AIILines.AddNew(range);
			aiiLine3.US_InvAmount = 50m;
			AssertEquals(105m, InvoiceLine.AIILines.TotalInvoiceAmount);

			aiiLine2.US_InvAmount = 40m;
			AssertEquals(110m, InvoiceLine.AIILines.TotalInvoiceAmount);
		}

		public void TestTotalInvoiceQty()
		{
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			aiiLine1.US_InvQty = 20m;
			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew(range);
			aiiLine2.US_InvQty = 35m;
			AIILine aiiLine3 = InvoiceLine.AIILines.AddNew(range);
			aiiLine3.US_InvQty = 50m;
			AssertEquals(105m, InvoiceLine.AIILines.TotalInvoiceQty);

			aiiLine2.US_InvQty = 40m;
			AssertEquals(110m, InvoiceLine.AIILines.TotalInvoiceQty);
		}

		public void TestTotalFirstCustomsQty()
		{
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			aiiLine1.US_CustomsQty = 20m;
			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew(range);
			aiiLine2.US_CustomsQty = 35m;
			AIILine aiiLine3 = InvoiceLine.AIILines.AddNew(range);
			aiiLine3.US_CustomsQty = 50m;
			AssertEquals(105m, InvoiceLine.AIILines.TotalFirstCustomsQty(false));

			aiiLine2.US_CustomsQty = 40m;
			AssertEquals(110m, InvoiceLine.AIILines.TotalFirstCustomsQty(false));
		}

		public void TestTotalSecondCustomsQty()
		{
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			aiiLine1.US_SecondQty = 20m;
			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew(range);
			aiiLine2.US_SecondQty = 35m;
			AIILine aiiLine3 = InvoiceLine.AIILines.AddNew(range);
			aiiLine3.US_SecondQty = 50m;
			AssertEquals(105m, InvoiceLine.AIILines.TotalSecondCustomsQty(false));

			aiiLine2.US_SecondQty = 40m;
			AssertEquals(110m, InvoiceLine.AIILines.TotalSecondCustomsQty(false));
		}

		public void TestTotalThirdCustomsQty()
		{
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			aiiLine1.US_ThirdQty = 20m;
			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew(range);
			aiiLine2.US_ThirdQty = 35m;
			AIILine aiiLine3 = InvoiceLine.AIILines.AddNew(range);
			aiiLine3.US_ThirdQty = 50m;
			AssertEquals(105m, InvoiceLine.AIILines.TotalThirdCustomsQty(false));

			aiiLine2.US_ThirdQty = 40m;
			AssertEquals(110m, InvoiceLine.AIILines.TotalThirdCustomsQty(false));
		}

		public void TestAddNew_WithParameters()
		{
			InvoiceLineGroupingRange range = InvoiceLine.LineGroupingRanges.AddNew();
			AIILine aiiLine1 = InvoiceLine.AIILines.AddNew(range);
			AssertEquals(range.PK, aiiLine1.US_CY_LineGroupRef);

			AIILine aiiLine2 = InvoiceLine.AIILines.AddNew();
			AssertEquals(ZGuid.Empty, aiiLine2.US_CY_LineGroupRef);
		}

		public void TestCollectionContainAIILine()
		{
			var aiiLine1 = InvoiceLine.AIILines.AddNew();
			var aiiLine2 = InvoiceLine.AIILines.AddNew();

			var secondInvoiceLine = InvoiceLine.AddSecondaryInvoiceLine();
			var aiiLine3 = secondInvoiceLine.AIILines.AddNew();
			var aiiLine4 = secondInvoiceLine.AIILines.AddNew();

			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var aiiLine5 = invoiceLine2.AIILines.AddNew();
			var aiiLine6 = invoiceLine2.AIILines.AddNew();

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var aiiLine7 = invoiceLine3.AIILines.AddNew();
			var aiiLine8 = invoiceLine3.AIILines.AddNew();

			var collection = new AIILineCollection(InvoiceLine);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new TypedEnumerable<AIILine>(new AIILine[] { aiiLine1, aiiLine2 }), new TypedEnumerable<AIILine>(collection));

			collection = new AIILineCollection(secondInvoiceLine);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new TypedEnumerable<AIILine>(new AIILine[] { aiiLine3, aiiLine4 }), new TypedEnumerable<AIILine>(collection));

			collection = new AIILineCollection(invoiceLine2);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new TypedEnumerable<AIILine>(new AIILine[] { aiiLine5, aiiLine6 }), new TypedEnumerable<AIILine>(collection));

			collection = new AIILineCollection(invoiceLine3);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new TypedEnumerable<AIILine>(new AIILine[] { aiiLine7, aiiLine8 }), new TypedEnumerable<AIILine>(collection));
		}

		public void TestUnitPriceAnd98()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_IsInvoiceByRequest = true;
			Declaration.US_EnableAII = true;
			Invoice.US_IsLineGrouping = true;
			InvoiceLine.UnitPrice = 5m;
			InvoiceLine.US_98InvCurrPerUnit = 3m;

			AIILineCollection collection = InvoiceLine.AIILines;
			AIILine aiiLine1 = collection.AddNew();
			AssertEquals(5m, aiiLine1.US_UnitPrice);
			AssertEquals(3m, aiiLine1.US_98InvCurrPerUnit);
			AIILine aiiLine2 = collection.AddNew();
			AssertEquals(5m, aiiLine2.US_UnitPrice);
			AssertEquals(3m, aiiLine2.US_98InvCurrPerUnit);
			AIILine aiiLine3 = collection.AddNew();
			AssertEquals(5m, aiiLine3.US_UnitPrice);
			AssertEquals(3m, aiiLine3.US_98InvCurrPerUnit);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AIILineCollection(InvoiceLine);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
