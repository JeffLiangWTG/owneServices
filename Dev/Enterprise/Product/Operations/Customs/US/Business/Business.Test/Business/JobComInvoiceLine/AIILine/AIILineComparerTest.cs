using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class AIILineComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_IsInvoiceByRequest = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_IsLineGrouping = true;
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			InvoiceLineGroupingRange range1 = invoiceLine1.LineGroupingRanges.AddNew(1, 2);
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			InvoiceLineGroupingRange range2 = invoiceLine2.LineGroupingRanges.AddNew(1, 3);

			InvoiceLineGroupingRange range3 = invoiceLine1.LineGroupingRanges.AddNew(2, 2);

			List<AIILine> list = new List<AIILine>(2);
			AIILine aiiLineX = Factory.New<AIILine>();
			list.Add(aiiLineX);
			aiiLineX.US_LineNo = 1;
			AIILine aiiLineY = Factory.New<AIILine>();
			list.Add(aiiLineY);
			aiiLineY.US_LineNo = 1;
			AIILineComparer comparer = new AIILineComparer();
			AssertEquals(0, comparer.Compare(aiiLineX, aiiLineX));
			AssertEquals(0, comparer.Compare(aiiLineY, aiiLineY));

			aiiLineY.US_LineNo = 0;
			Assert(list, comparer, aiiLineY, aiiLineX);
			aiiLineX.US_LineNo = 0;
			aiiLineY.US_LineNo = 1;
			Assert(list, comparer, aiiLineX, aiiLineY);
			aiiLineX.US_LineNo = 1;
			AssertEquals(0, comparer.Compare(aiiLineX, aiiLineY));

			aiiLineX.US_CY_LineGroupRef = range1.PK;
			Assert(list, comparer, aiiLineX, aiiLineY);
			aiiLineX.US_CY_LineGroupRef = ZGuid.Empty;
			aiiLineY.US_CY_LineGroupRef = range1.PK;
			Assert(list, comparer, aiiLineY, aiiLineX);
			aiiLineX.US_CY_LineGroupRef = range1.PK;
			AssertEquals(0, comparer.Compare(aiiLineX, aiiLineY));

			aiiLineX.US_CY_LineGroupRef = range2.PK;
			Assert(list, comparer, aiiLineY, aiiLineX);
			aiiLineX.US_CY_LineGroupRef = range1.PK;
			aiiLineY.US_CY_LineGroupRef = range2.PK;
			Assert(list, comparer, aiiLineX, aiiLineY);
			aiiLineX.US_CY_LineGroupRef = range2.PK;
			AssertEquals(0, comparer.Compare(aiiLineX, aiiLineY));

			aiiLineX.US_CY_LineGroupRef = range3.PK;
			Assert(list, comparer, aiiLineY, aiiLineX);
			aiiLineX.US_CY_LineGroupRef = range2.PK;
			aiiLineY.US_CY_LineGroupRef = range3.PK;
			Assert(list, comparer, aiiLineX, aiiLineY);
			aiiLineX.US_CY_LineGroupRef = range3.PK;
			AssertEquals(0, comparer.Compare(aiiLineX, aiiLineY));
		}

		void Assert(List<AIILine> list, AIILineComparer comparer, AIILine aiiLine1, AIILine aiiLine2)
		{
			list.Sort(comparer);
			AssertEquals(aiiLine1, list[0]);
			AssertEquals(aiiLine2, list[1]);
		}

		public void TestEquals()
		{
			AssertEquals(true, new AIILineComparer().Equals(new AIILineComparer()));
		}

		public void TestGetHashCode()
		{
			int hashCode1 = new AIILineComparer().GetHashCode();
			int hashCode2 = new AIILineComparer().GetHashCode();
			AssertEquals(hashCode1, hashCode2);
			AssertNotEquals(0, hashCode1);
		}
	}
}
