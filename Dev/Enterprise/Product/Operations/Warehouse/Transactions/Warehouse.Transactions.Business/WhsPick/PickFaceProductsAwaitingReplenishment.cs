using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickFaceProductsAwaitingReplenishment : NonPersistentBusinessObject
	{
		public PickFaceProductsAwaitingReplenishment(ZGuid clientPK, ZGuid productPK, BusinessObjectFactory factory) : base(factory)
		{
			ClientPK = clientPK;
			ProductPK = productPK;
		}

		ZGuid ClientPK { get; }
		ZGuid ProductPK { get; }

		#region Properties

		public WhsPickFaceAwaitingReplenishmentViewCollection PickFacesAwaitingReplenishmentWithSameWhsClientProduct
		{
			get
			{
				return pickFacesAwaitingReplenishmentWithSameWhsClientProduct ??
						(pickFacesAwaitingReplenishmentWithSameWhsClientProduct =
							new WhsPickFaceAwaitingReplenishmentViewCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickFaceAwaitingReplenishmentView))));
			}
		}

		WhsPickFaceAwaitingReplenishmentViewCollection pickFacesAwaitingReplenishmentWithSameWhsClientProduct;

		public ZString ClientCode => Factory.Load<OrgHeader>(ClientPK)?.OH_Code ?? "";
		public ZString ProductCode => Factory.Load<OrgSupplierPart>(ProductPK)?.OP_PartNum ?? "";

		#endregion
	}
}
