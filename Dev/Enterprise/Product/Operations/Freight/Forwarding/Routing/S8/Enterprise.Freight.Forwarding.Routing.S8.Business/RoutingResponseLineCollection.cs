using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingResponseLineCollection : NonPersistentBusinessObjectCollection<RoutingResponseLine>
	{
		public RoutingResponseLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RoutingResponseLine("", Factory);
		}

		protected override bool AllowNewCore { get { return false; } }
		protected override bool AllowRemoveCore { get { return false; } }
	}
}
