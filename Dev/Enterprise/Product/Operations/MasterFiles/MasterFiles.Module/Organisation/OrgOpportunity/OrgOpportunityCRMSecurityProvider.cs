using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgOpportunityCRMSecurityProvider : CRMSecurityProvider<OrgOpportunity>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { OrgOpportunitySchema.P8_OH };

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson };

		protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.OpportunityManagementCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
