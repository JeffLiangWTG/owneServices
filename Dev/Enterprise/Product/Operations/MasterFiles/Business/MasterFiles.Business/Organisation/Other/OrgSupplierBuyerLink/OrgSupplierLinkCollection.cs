using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierLinkCollection : OrgSupplierBuyerLinkDependentCollection
	{
		public OrgSupplierLinkCollection(OrgHeader buyer, BusinessObjectFactory factory) : base(buyer, factory)
		{
		}

		public virtual OrgSupplierBuyerLink AddNew(OrgHeader supplier)
		{
			OrgSupplierBuyerLink link = AddNew();
			link.OL_OH_Supplier = supplier.PK;
			return link;
		}

		#region GetDefaultIncoTerm

		protected override ZString GetDefaultIncoTerm()
		{
			return Buyer.MiscServ.OM_IMDefaultINCOTerm;
		}

		#endregion

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgSupplierBuyerLinkSchema.OL_OH_Buyer; }
		}

		OrgHeader Buyer
		{
			get { return Master; }
		}

		#endregion
	}
}
