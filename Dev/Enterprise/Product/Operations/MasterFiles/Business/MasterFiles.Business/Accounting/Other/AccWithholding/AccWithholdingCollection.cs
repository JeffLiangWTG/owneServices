using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccWithholding)]
	public class AccWithholdingCollection : BusinessObjectCollection<AccWithholding>
	{
		public AccWithholdingCollection(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public AccWithholdingCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory)
		{
			CompanyPK = company != null ? company.PK : GlbCompany.CurrentCompany.PK;
		}

		readonly ZGuid CompanyPK;

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccWithholdingSchema.AW_GC, SQLComparisonOperator.Equal, CompanyPK);
		}
	}
}
