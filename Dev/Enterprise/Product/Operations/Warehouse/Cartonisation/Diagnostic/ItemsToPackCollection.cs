using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class ItemsToPackCollection : NonPersistentBusinessObjectCollection<DummyCartonisableItem>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyCartonisableItem();
		}
	}
}

