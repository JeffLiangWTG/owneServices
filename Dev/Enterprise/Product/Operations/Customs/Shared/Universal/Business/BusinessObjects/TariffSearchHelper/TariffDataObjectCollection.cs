using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class TariffDataObjectCollection : NonPersistentBusinessObjectCollection<TariffDataObject>
	{
		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;
	}
}
