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
	public class WorkItemCRMSecurityProvider : CRMSecurityProvider<WorkItem>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { WorkItemSchema.WKI_SystemCreateUser };

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.WorkItemCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}
