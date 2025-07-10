using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USOrgSupplierBuyerLinkAddInfoCollection : NonPersistentBusinessObjectCollection<USOrgSupplierBuyerLinkAddInfo>
	{
		public USOrgSupplierBuyerLinkAddInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
