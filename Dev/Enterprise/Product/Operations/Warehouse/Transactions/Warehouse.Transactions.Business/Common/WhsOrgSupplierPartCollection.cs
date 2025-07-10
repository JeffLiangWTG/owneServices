using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsConfigProduct)]
	public class WhsOrgSupplierPartCollection : OrgSupplierPartCollection, IWhsOrgSupplierPartCollection
	{
		public WhsOrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsOrgSupplierPartCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, supplier, owner, isExport)
		{
		}

		public WhsOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport, PartFilterOptions filterOptions)
			: base(factory, supplier, owner, isExport, filterOptions)
		{
		}

		public WhsOrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, ZString productDescription, ZString stockKeepingUnits, bool isExport)
			: base(factory, supplier, owner, productDescription, stockKeepingUnits, isExport)
		{
		}
	}
}
