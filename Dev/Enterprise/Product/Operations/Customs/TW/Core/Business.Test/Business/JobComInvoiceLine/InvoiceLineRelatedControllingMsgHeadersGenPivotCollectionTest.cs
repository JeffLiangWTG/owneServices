using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineRelatedControllingMsgHeadersGenPivotCollection))]
	sealed class InvoiceLineRelatedControllingMsgHeadersGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetRelatedPivotAndContains()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			var genPivots = new InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(jobComInvoiceLine);
			var caHeader1 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader1);
			var caHeader2 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader2);
			AssertEquals(genPivots[0], genPivots.GetRelatedPivot(caHeader1));
			AssertEquals(true, genPivots.Contains(caHeader2));
		}

		public void TestAddPivotFor()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			var genPivots = new InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(jobComInvoiceLine);
			var caHeader1 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader1);
			AssertEquals(1, genPivots.Count);
			var caHeader2 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader2);
			AssertEquals(2, genPivots.Count);
		}

		public void TestDeletePivotFor()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			var genPivots = new InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(jobComInvoiceLine);
			var caHeader1 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader1);
			var caHeader2 = Factory.New<CusTWControllingMessageHeader>();
			genPivots.AddPivotFor(caHeader2);
			AssertEquals(2, genPivots.Count);
			genPivots.DeletePivotFor(Factory.New<CusTWControllingMessageHeader>());
			AssertEquals(2, genPivots.Count);
			genPivots.DeletePivotFor(caHeader2);
			AssertEquals(false, genPivots.Contains(caHeader2));
			AssertEquals(true, genPivots.Contains(caHeader1));
			AssertEquals(1, genPivots.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(Factory.New<JobComInvoiceLine>());
		}
	}
}
