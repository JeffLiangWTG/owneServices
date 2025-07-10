using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SalesMatchingDataCollection : NonPersistentBusinessObjectCollection<SalesMatchingData>
	{
		public SalesMatchingDataCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesMatchingData();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
