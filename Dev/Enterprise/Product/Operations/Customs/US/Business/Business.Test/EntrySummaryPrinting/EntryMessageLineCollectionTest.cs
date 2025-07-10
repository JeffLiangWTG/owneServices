using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting.Testing
{
	[TestedType(typeof(EntryMessageLineCollection))]
	sealed class EntryMessageLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryMessageLineCollection>
	{
		protected override EntryMessageLineCollection GetCollectionToTest() => new EntryMessageLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryMessageLine(Factory);
	}
}
