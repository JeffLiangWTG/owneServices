using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectCRMSecurityProvider : CRMSecurityProvider<Project>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { WorkProjectSchema.WKP_GS_NKProjectManager };

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { WorkProjectSchema.WKP_OA_ClientAddress };

		public override CRMSecurity CRMSecurity => Env.Security.ProjectCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
