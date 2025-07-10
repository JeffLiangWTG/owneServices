using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderLineLookups : WhsComponentOrderLineLookups
	{
		public WhsWorkOrderLineLookups(WhsWorkOrderLine parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsWorkOrderLine Parent
		{
			get { return (WhsWorkOrderLine)base.Parent; }
		}

		#endregion

		#region OrgSupplierPart

		protected override OrgSupplierPartCollection GetNewOrgSupplierPartCollection()
		{
			return new WhsOrgSupplierPartCollectionBOM(Factory);
		}

		protected override OrgSupplierPartCollection GetNewOrgSupplierPartCollectionWithClient(OrgHeader client)
		{
			var filterOptions = ExcludeProductsNotForResale ? PartFilterOptions.ExcludeNotForResale : PartFilterOptions.None;
			return new WhsOrgSupplierPartCollectionBOM(Factory, null, client, false, filterOptions);
		}

		#endregion
	}
}
