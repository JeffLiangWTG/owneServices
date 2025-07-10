using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	class GlbCompanyCampaignCRMSecurityProvider : CRMSecurityProvider<GlbCompanyCampaign>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator, GlbCompanyCampaignSchema.G0_GS_NKCampaignManager };

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.CampaignManagementCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
