using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	class SalesDashboardOneOffQuoteCRMSecurityProvider : CRMSecurityProvider<RateOneOffShipment>
	{
		public override CRMSecurity CRMSecurity => Env.Security.OneOffQuoteCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes => new ZString[] { AutoDocAddressTypes.Codes.QuotationClientAddress };

		protected override bool ShouldCheckJobHeader => true;

		public override Tuple<Type, SchemaGuidColumn> SecurityTargetObjectReference => Tuple.Create(typeof(ViewQuotedBooking), RateOneOffShipmentSchema.TT_TH);
	}
}
