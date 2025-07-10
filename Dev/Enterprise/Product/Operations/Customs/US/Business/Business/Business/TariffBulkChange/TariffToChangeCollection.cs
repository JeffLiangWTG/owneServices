using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class TariffToChangeCollection : NonPersistentBusinessObjectCollection<TariffToChange>, IObsoleteValidation
	{
		public TariffToChangeCollection(USTariffBulkChange parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}
		readonly USTariffBulkChange parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TariffToChange(parent);
		}
	}
}
