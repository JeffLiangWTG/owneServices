using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveLineLookups : WhsDocketLineLookups
	{
		public WhsReceiveLineLookups(WhsReceiveLine parent)
			: base(parent)
		{
		}

		#region GetNewOrgSupplierPartCollectionWithClient

		public override OrgSupplierPartCollection SupplierParts
		{
			get
			{
				OrgSupplierPartCollection supplierParts = null;

				if (Parent.IsTemporaryProduct)
				{
					// If there is a temporary product, we can't cache the collection as it relies on per line fields
					var client = Parent.Docket?.Client;

					if (client != null)
					{
						supplierParts = GetNewOrgSupplierPartCollectionWithClient(client);
					}
				}

				return supplierParts ?? base.SupplierParts;
			}
		}

		protected override OrgSupplierPartCollection GetNewOrgSupplierPartCollectionWithClient(OrgHeader client)
		{
			return new WhsOrgSupplierPartCollection(Factory, null, Parent?.Docket.Client, Parent.ProductDesc, Parent.ProductUQ, false);
		}

		#endregion
	}
}
