using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501BillCollection))]
	sealed class EntrySummary7501BillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntrySummary7501BillCollection>
	{
		protected override EntrySummary7501BillCollection GetCollectionToTest() => new EntrySummary7501BillCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Factory.New<CusEntryHeader>();
			var bill = Factory.New<Bill>();
			return new EntryHeaderENS7501Bill(entry.PK, bill.ITAndSplitDetails.AddNew());
		}
	}
}
