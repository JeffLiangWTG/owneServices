using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobShipmentCRMSecurityProvider : CRMSecurityProvider<ForwardingShipment>
	{
		public override CRMSecurity CRMSecurity => Env.Security.MaintainShipmentCRMSecurity;

		public override CodePairRegistryItem OSMGSecurityLevelRegistryItem => RawDataRegistry.Instance.ShipmentOSMGSecurityLevel;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => true;

		protected override IEnumerable<ZString> RelatedJobDocAddressTypes
		{
			get => new ZString[] { AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress, AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress, AutoDocAddressTypes.Codes.ControllingCustomer };
		}

		protected override ZDBOnlyQuery GetOSMGQueryEnhancedSecurityLevel(GlbStaff staff, SchemaColumn pkColumn)
		{
			var parameters = new ZSqlParameterCollection
			{
				{ "@CompanyPK", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK },
				{ "@StaffPK", staff.PK, GlbStaffSchema.PK }
			};

			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			query.AddFilterAndZSQLParameterCollection($"(SELECT HasAccess FROM HasEnhancedAccessOSMGShipment({pkColumn.Name}, @CompanyPK, @StaffPK)) = 1", parameters);

			return query;
		}
	}
}
