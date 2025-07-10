using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class DummyPackLineCollection : NonPersistentBusinessObjectCollection<DummyPackLine>
	{
		public DummyPackLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyPackLine(Factory);
		}
	}
}
