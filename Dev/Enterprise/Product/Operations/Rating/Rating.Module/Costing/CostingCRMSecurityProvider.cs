using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class CostingCRMSecurityProvider : CRMSecurityProvider<Costing>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { RatingHeaderSchema.TH_OH };

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		public override CRMSecurity CRMSecurity => Env.Security.CostingRatesCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;
	}
}


