using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class BulkRateUpdaterContainerTypeCollection : NonPersistentBusinessObjectCollection<BulkRateUpdaterContainerType>
	{
		readonly RateEntryLookups lookups;
		readonly RefContainerCollection modeRestrictedContainersForValidation;

		public BulkRateUpdaterContainerTypeCollection(BusinessObjectFactory factory, RateEntryLookups lookups, RefContainerCollection modeRestrictedContainersForValidation) : base(factory)
		{
			this.lookups = lookups;
			this.modeRestrictedContainersForValidation = modeRestrictedContainersForValidation;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BulkRateUpdaterContainerType(Factory, lookups, modeRestrictedContainersForValidation);
		}
	}
}
