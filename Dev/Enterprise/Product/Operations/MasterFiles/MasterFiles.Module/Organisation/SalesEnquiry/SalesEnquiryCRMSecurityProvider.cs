using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class SalesEnquiryCRMSecurityProvider : CRMSecurityProvider<SalesEnquiry>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { OrgColdCallRegisterSchema.O1_GS_NKRepAssigned };

		protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { OrgColdCallRegisterSchema.O1_OA_LinkedAddress };

		public override CRMSecurity CRMSecurity => Env.Security.InquiryManagerCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;

		public override bool ShouldReturnEmptyOrgAddress => false;
	}
}
