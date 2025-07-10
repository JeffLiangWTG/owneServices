using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Module
{
	class BillOfLadingCRMSecurityProvider : CRMSecurityProvider<BillOfLading>
	{
		public override CRMSecurity CRMSecurity => Env.Security.AgencyBillOfLadingCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes
		{
			get => new ZString[] { AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress, AutoDocAddressTypes.Codes.ControllingCustomer };
		}
	}
}
