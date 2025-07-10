using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class CartonsCollection : NonPersistentBusinessObjectCollection<DummyCartonDefinition>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyCartonDefinition();
		}
	}
}

