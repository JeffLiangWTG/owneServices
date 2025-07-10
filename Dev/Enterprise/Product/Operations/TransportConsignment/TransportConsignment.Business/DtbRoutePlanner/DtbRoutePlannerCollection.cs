using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbRoutePlannerCollection : NonPersistentBusinessObjectCollection<DtbRoutePlanner>
	{
		DtbRoutePlannerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DtbRoutePlannerCollection New(BusinessObjectFactory factory)
		{
			var planners = new DtbRoutePlannerCollection(factory);
			planners.AddNew();
			return planners;
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DtbRoutePlanner(Factory);
		}

		#endregion

		#region SwapFactoryRemoveAllAndAddNew

		public void SwapFactoryRemoveAllAndAddNew()
		{
			using (SuspendListChanged())
			{
				var currentLegPlanner = this[0];
				var newFactory = new BusinessObjectFactory();
				SwapFactoryAndRemoveAll(newFactory);
				currentLegPlanner.CopyTransientProperties(AddNew());
			}
		}

		#endregion
	}
}
