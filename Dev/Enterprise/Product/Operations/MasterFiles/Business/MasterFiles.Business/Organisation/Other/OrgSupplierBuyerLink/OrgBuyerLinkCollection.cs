using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgBuyerLinkCollection : OrgSupplierBuyerLinkDependentCollection
	{
		public OrgBuyerLinkCollection(OrgHeader supplier, BusinessObjectFactory factory)
			: base(supplier, factory)
		{
		}

		public virtual OrgSupplierBuyerLink AddNew(OrgHeader buyer)
		{
			OrgSupplierBuyerLink link = AddNew();
			link.OL_OH_Buyer = buyer.PK;
			return link;
		}

		#region GetDefaultIncoTerm

		protected override ZString GetDefaultIncoTerm()
		{
			return Supplier.MiscServ.OM_EXDefaultIncoTerm;
		}

		#endregion

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgSupplierBuyerLinkSchema.OL_OH_Supplier; }
		}

		OrgHeader Supplier
		{
			get { return Master; }
		}

		#endregion
	}
}
