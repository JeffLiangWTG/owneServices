using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	class GlbCompanyCampaignItemCRMSecurityProvider : CRMSecurityProvider<GlbCompanyCampaignItem>
	{
		public override CRMSecurity CRMSecurity => Env.Security.CampaignManagementCRMSecurity;

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => false;

		public override Tuple<Type, SchemaGuidColumn> SecurityTargetObjectReference => Tuple.Create(typeof(GlbCompanyCampaign), GlbCompanyCampaignItemSchema.G8_G0);

		protected override void AppendAdditionalStaffNKQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery staffNKSecurityQuery, bool notIn, JoinCondition joinCondition)
		{
			var campaignQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignSchema.PK, notIn);
			campaignQuery.AddSubQuery(GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator, staffNKSecurityQuery, JoinCondition.Or);
			campaignQuery.AddSubQuery(GlbCompanyCampaignSchema.G0_GS_NKCampaignManager, staffNKSecurityQuery, JoinCondition.Or);
			mainQuery.AddSubQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignQuery, joinCondition);
		}
	}
}
