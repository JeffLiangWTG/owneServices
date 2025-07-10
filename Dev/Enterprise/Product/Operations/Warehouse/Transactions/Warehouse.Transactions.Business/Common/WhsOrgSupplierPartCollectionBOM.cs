using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsConfigProduct)]
	public class WhsOrgSupplierPartCollectionBOM : OrgSupplierPartCollectionBOM
	{
		public WhsOrgSupplierPartCollectionBOM(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsOrgSupplierPartCollectionBOM(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport, PartFilterOptions filterOptions = PartFilterOptions.None)
			: base(factory, supplier, owner, isExport, filterOptions)
		{
		}
	}
}
