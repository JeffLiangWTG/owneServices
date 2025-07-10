using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrderLineCRMSecurityProvider : CRMSecurityProvider<OrderLine>
	{
		public override CRMSecurity CRMSecurity => Env.Security.OrderLineTrackingCRMSecurity;

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override void AppendAdditionalOrgQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery orgSecurityQuery, bool notIn, JoinCondition joinCondition)
		{
			var orderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK, notIn);
			var buyerAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			buyerAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgSecurityQuery, JoinCondition.And);
			orderQuery.AddSubQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerAddressSubQuery, joinCondition);
			mainQuery.AddSubQuery(JobOrderLineSchema.JO_JD, orderQuery, JoinCondition.Or);
		}

		protected override IEnumerable<OrgHeader> GetRelatedOrgHeaders(OrderLine businessObject)
		{
			var buyer = businessObject?.Order?.Buyer;
			return buyer != null ? new OrgHeader[] { buyer } : Enumerable.Empty<OrgHeader>();
		}

		protected override bool ShouldCheckJobHeader => false;
	}
}
