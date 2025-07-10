using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Confirmations.Module
{
	class ConsolidatedTransportBookingCRMSecurityProvider : CRMSecurityProvider<CommonConsolidatedTransportBooking>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { JobConsolidatedTransportBookingSchema.D1_OA_Customer };

		public override CRMSecurity CRMSecurity => Env.Security.ConsolidatedTransportBookingCRMSecurity;

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override bool ShouldCheckJobHeader => true;
	}
}
