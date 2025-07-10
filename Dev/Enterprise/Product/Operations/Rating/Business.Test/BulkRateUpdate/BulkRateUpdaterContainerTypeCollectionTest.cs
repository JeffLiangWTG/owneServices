using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(BulkRateUpdaterContainerTypeCollection))]
	internal class BulkRateUpdaterContainerTypeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BulkRateUpdaterContainerTypeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}

		readonly ZQuery transportModeFilter = new ();

		public RefContainerCollection ContainersForValidation => new RefContainerCollection(Factory, transportModeFilter);

		protected override BulkRateUpdaterContainerTypeCollection GetCollectionToTest() => new BulkRateUpdaterContainerTypeCollection(Factory, new RateEntryLookups(Factory.NewWithValidTestData<RateEntry>()), ContainersForValidation);
	}
}
