using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class RunSheetDashboardCollection : NonPersistentBusinessObjectCollection<RunSheetDashboard>
	{
		RunSheetDashboardCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static RunSheetDashboardCollection New(BusinessObjectFactory factory)
		{
			var planners = new RunSheetDashboardCollection(factory);
			planners.AddNew();
			return planners;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RunSheetDashboard(Factory);
		}

		public void SwapFactoryRemoveAllAndAddNew()
		{
			using (SuspendListChanged())
			{
				var currentRunSheetDashboard = this[0];
				var newFactory = new BusinessObjectFactory();
				var newDashboard = new RunSheetDashboard(newFactory);
				currentRunSheetDashboard.CopyTransientProperties(newDashboard);
				newDashboard.UpdateFilter();
				SwapFactoryAndRemoveAll(newFactory);
				Add(newDashboard);
			}
		}
	}
}
