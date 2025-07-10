using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReceiveCRMSecurityProvider : CRMSecurityProvider<WhsReceive>
	{
		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { WhsDocketSchema.WD_OH_Client };

		public override CRMSecurity CRMSecurity => Env.Security.WhsReceiveCRMSecurity;

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override bool ShouldCheckJobHeader => true;
	}
}
