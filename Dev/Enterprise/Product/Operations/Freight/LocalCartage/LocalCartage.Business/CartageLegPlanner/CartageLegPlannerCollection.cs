using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegPlannerCollection : NonPersistentBusinessObjectCollection<CartageLegPlanner>
	{
		/// <summary>
		/// Auto Adds 1 CartageLegPlanner
		/// Used for funky binding
		/// </summary>
		/// <param name="factory"></param>
		public CartageLegPlannerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Add(new CartageLegPlanner(factory));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CartageLegPlanner(Factory);
		}

		protected override CargoWise.Types.ZString HumanReadableNameCore
		{
			get { return Res.GetString("520b23a7-c472-47d1-8589-c2c99455a678", "Port Transport Leg Planner"); }
		}

		public void SwapFactoryRemoveAllAndAddNew()
		{
			using (SuspendListChanged())
			{
				var currentLegPlanner = this[0];
				var newFactory = new BusinessObjectFactory();
				var provider = CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory);
				CommonCartageBehaviorStrategyProvider.SetProvider(newFactory, provider);
				SwapFactoryAndRemoveAll(newFactory);
				AddNew().SwapWith(currentLegPlanner);
				currentLegPlanner.CartageLegs.RemoveAll();
				currentLegPlanner.Delete();
			}
		}
	}
}
