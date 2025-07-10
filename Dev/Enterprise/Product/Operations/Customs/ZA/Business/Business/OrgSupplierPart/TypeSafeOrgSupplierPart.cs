using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class TypeSafeOrgSupplierPart : Customs.Business.OrgSupplierPart
	{
		protected TypeSafeOrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new OrgSupplierPartLookups Lookups
		{
			get { return base.Lookups as OrgSupplierPartLookups; }
		}

		protected override MasterFiles.Business.OrgSupplierPartLookups GetNewLookups()
		{
			return new OrgSupplierPartLookups(this as OrgSupplierPart);
		}
	}
}
