using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersCRMSecurityProvider : CRMSecurityProvider<Order>
	{
		public override CRMSecurity CRMSecurity => Env.Security.OrderTrackingCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { JobOrderHeaderSchema.JD_OA_BuyerAddress };

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => false;
	}
}
