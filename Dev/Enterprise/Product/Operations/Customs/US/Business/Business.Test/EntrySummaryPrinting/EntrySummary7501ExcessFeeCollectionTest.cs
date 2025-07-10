using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501ExcessFeeCollection))]
	sealed class EntrySummary7501ExcessFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntrySummary7501ExcessFeeCollection>
	{
		protected override EntrySummary7501ExcessFeeCollection GetCollectionToTest() => new EntrySummary7501ExcessFeeCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Factory.New<CusEntryHeader>();
			var chargeFee = entry.Charges.AddNew();
			return new EntryHeader7501ExcessFee(entry, chargeFee);
		}
	}
}
