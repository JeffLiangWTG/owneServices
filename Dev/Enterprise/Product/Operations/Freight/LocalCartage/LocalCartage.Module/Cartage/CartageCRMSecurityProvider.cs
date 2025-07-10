using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageCRMSecurityProvider : CRMSecurityProvider<CommonCartage>
	{
		public override CRMSecurity CRMSecurity => Env.Security.TransportJobCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes
		{
			get => new ZString[] { AutoDocAddressTypes.Codes.LocalCartageImporter, AutoDocAddressTypes.Codes.LocalCartageExporter };
		}
	}
}
