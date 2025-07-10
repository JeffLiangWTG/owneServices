using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeProspectByCompetitorCollection : DependentBusinessObjectCollection<OrgTradeProspect, OrgHeader>
	{
		public OrgTradeProspectByCompetitorCollection(OrgHeader competitor)
			: base(competitor)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgTradeProspectSchema.PAP_OH_Competitor; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Master

		public new OrgHeader Master
		{
			get { return base.Master; }
		}

		#endregion
	}
}
