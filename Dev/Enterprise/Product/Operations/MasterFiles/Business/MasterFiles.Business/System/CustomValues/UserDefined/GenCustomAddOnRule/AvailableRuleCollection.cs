using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class AvailableRuleCollection : NonPersistentBusinessObjectCollection<AvailableRule>
	{
		public AvailableRuleCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AvailableRule(Factory);
		}

		protected override bool AllowNewCore { get { return false; } }
		protected override bool AllowRemoveCore { get { return false; } }
	}
}
