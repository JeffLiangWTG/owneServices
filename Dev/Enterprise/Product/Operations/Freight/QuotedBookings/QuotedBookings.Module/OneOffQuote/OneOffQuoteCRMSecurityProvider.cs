using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class OneOffQuoteCRMSecurityProvider : CRMSecurityProvider<ViewQuotedBooking>
	{
		public override CRMSecurity CRMSecurity => Env.Security.OneOffQuoteCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<OrgHeader> GetRelatedOrgHeaders(ViewQuotedBooking businessObject)
		{
			var result = base.GetRelatedOrgHeaders(businessObject);
			var org = businessObject.QuotedBooking?.Client;

			if (org != null)
			{
				result = result.Union(new OrgHeader[] { org });
			}

			return result;
		}

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes => new ZString[] { AutoDocAddressTypes.Codes.QuotationClientAddress };

		protected override bool ShouldCheckJobHeader => true;
	}
}
