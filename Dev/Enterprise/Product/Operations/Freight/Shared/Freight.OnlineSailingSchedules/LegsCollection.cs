using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class LegsCollection : NonPersistentBusinessObjectCollection<Leg>
	{
		public LegsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Leg(Factory);
		}
	}
}
