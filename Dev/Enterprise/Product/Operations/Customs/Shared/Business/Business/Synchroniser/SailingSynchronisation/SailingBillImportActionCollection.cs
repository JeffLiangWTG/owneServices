using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class SailingBillImportActionCollection<T> : NonPersistentBusinessObjectCollection<T> where T : SailingBillImportAction
	{
		protected SailingBillImportActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
