using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Module
{
	class AgencyBookingCRMSecurityProvider : CRMSecurityProvider<AgencyBooking>
	{
		protected override IEnumerable<OrgHeader> GetRelatedOrgHeaders(AgencyBooking businessObject)
		{
			return new OrgHeader[] { businessObject.BookingParty }.Where(x => x != null);
		}

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes
		{
			get => new ZString[] { AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress, AutoDocAddressTypes.Codes.ControllingCustomer };
		}

		public override CRMSecurity CRMSecurity => Env.Security.AgencyBookingCRMSecurity;

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override bool ShouldCheckJobHeader => true;
	}
}
