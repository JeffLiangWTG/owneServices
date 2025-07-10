using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationCRMSecurityProvider : CRMSecurityProvider<BaseJobDeclaration>
	{
		public override CRMSecurity CRMSecurity => Env.Security.CustomsDeclarationEnquiryCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes => new ZString[] { AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress, AutoDocAddressTypes.Codes.ImportBroker };
	}
}
