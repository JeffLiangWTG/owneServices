using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501LineCollection))]
	sealed class EntrySummary7501LineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntrySummary7501LineCollection>
	{
		protected override EntrySummary7501LineCollection GetCollectionToTest() => new EntrySummary7501LineCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ACSEntryHeaderENS7501Line(Factory.NewWithValidTestData<CusEntryLine>(), false, false);
	}
}
