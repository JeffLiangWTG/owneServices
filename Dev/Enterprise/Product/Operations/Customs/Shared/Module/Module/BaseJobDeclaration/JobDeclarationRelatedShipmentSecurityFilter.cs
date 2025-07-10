using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationRelatedShipmentSecurityFilter : ModuleGuidFilter
	{
		public JobDeclarationRelatedShipmentSecurityFilter(BusinessObjectFactory factory)
			: base(DeclarationFilterConstants.RelatedShipmentSecurity, ModuleIDs.GlbStaff, GlbStaffSchema.PK, new GlbStaffCollection(factory))
		{
			Property = GlbStaff.CurrentUser.PK;
		}

		protected override ZQuery GetQuery()
		{
			return new JobDeclarationRelatedShipmentFilter(Factory).Query;
		}

		public override bool HasComparisonOperator => false;
	}
}
