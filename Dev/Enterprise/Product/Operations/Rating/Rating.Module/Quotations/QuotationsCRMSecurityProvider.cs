using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Security;

namespace Enterprise.Rating.Module
{
	public class QuotationsCRMSecurityProvider : CRMSecurityProvider<Quote>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<OrgHeader> GetRelatedOrgHeaders(Quote businessObject)
		{
			var result = base.GetRelatedOrgHeaders(businessObject);
			var orgHeader = businessObject?.QuotationClientAddress?.Organisation;
			if (orgHeader != null)
			{
				result = result.Union(new OrgHeader[] { orgHeader });
			}

			return result;
		}

		public override CRMSecurity CRMSecurity => Env.Security.QuotationCRMSecurity;

		protected override bool ShouldCheckJobHeader => false;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes => new ZString[] { AutoDocAddressTypes.Codes.QuotationClientAddress };
	}
}


