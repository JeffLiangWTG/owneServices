using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class AllocationMethodDefaultRuleCollection : NonPersistentBusinessObjectCollection<AllocationMethodDefaultRule>
	{
		public AllocationMethodDefaultRuleCollection(BusinessObjectFactory factory)
			: base(factory) { }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AllocationMethodDefaultRule(Factory);
		}

		#endregion
	}
}
