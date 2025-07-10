using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsCRMSecurityProvider : CRMSecurityProvider<OrgContact>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { OrgContactSchema.OC_OH };

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.OrganisationCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;

		public override Tuple<Type, SchemaGuidColumn> SecurityTargetObjectReference => Tuple.Create(typeof(OrgHeader), OrgContactSchema.OC_OH);
	}
}
