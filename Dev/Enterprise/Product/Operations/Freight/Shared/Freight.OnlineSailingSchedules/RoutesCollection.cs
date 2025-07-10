using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	[ModuleID(ModuleId.OnlineSailingSchedules)]
	public class RoutesCollection : NonPersistentBusinessObjectCollection<Route>
	{
		public RoutesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Route(Factory);
		}

		public new void ReSort()
		{
			base.ReSort();
		}
	}
}
