using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class QuotedBookingCRMSecurityProvider : CRMSecurityProvider<ViewQuotedBooking>
	{
		public override CRMSecurity CRMSecurity => Env.Security.QuickBookingCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override bool AllowNullLocalAddress => true;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes => new ZString[] { AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress };
	}
}
