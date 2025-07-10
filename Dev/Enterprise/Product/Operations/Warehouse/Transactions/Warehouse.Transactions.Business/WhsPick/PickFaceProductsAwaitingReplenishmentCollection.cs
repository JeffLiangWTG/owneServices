using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickFaceProductsAwaitingReplenishmentCollection : NonPersistentBusinessObjectCollection<PickFaceProductsAwaitingReplenishment>
	{
		public PickFaceProductsAwaitingReplenishmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var client = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			return new PickFaceProductsAwaitingReplenishment(client.PK, product.PK, Factory);
		}
	}
}
