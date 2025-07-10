using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class OrganisationCRMSecurityProvider : CRMSecurityProvider<OrgHeader>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { OrgHeaderSchema.PK };

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.OrganisationCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
