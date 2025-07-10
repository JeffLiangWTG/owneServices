using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CommunicationCRMSecurityProvider : CRMSecurityProvider<OrgSalesCall>
	{
		protected override IEnumerable<GlbStaff> GetRelatedGlbStaffs(OrgSalesCall businessObject)
		{
			//Attendee
			var staffPKs = businessObject.AdditionalAttendeesStaff.OfType<OrgSalesCallAdditionalAttendee>()
					.Where(x => x.O6_AttendeeTableCode == GlbStaffSchema.Constants.Prefix).Select(x => x.O6_AttendeeID).Distinct();
			var query = new ZQuery(GlbStaffSchema.PK, staffPKs);

			return businessObject.Factory.Load<GlbStaff>(query).Union(base.GetRelatedGlbStaffs(businessObject));
		}

		protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { OrgSalesCallSchema.OQ_OH };

		protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => new SchemaColumn[] { OrgSalesCallSchema.OQ_GS_NKSalesRep };

		protected override void AppendAdditionalStaffPKQuery(ZDBOnlyQuery mainQuery, ZDBOnlySubQuery staffPKSecurityQuery, bool notIn, JoinCondition joinCondition)
		{
			var subqueryAttendee = new ZDBOnlySubQuery(typeof(OrgSalesCallAdditionalAttendee), OrgSalesCallAdditionalAttendeeSchema.O6_OQ, notIn);
			subqueryAttendee.AddToFilter(JoinCondition.And, OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeTableCode, GlbStaffSchema.Constants.Prefix);
			subqueryAttendee.AddSubQuery(OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeID, staffPKSecurityQuery, JoinCondition.And);
			mainQuery.AddSubQuery(OrgSalesCallSchema.PK, subqueryAttendee, joinCondition);
		}

		public override CRMSecurity CRMSecurity => Env.Security.CommunicationManagerCRMSecurity;

		protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

		protected override bool ShouldCheckJobHeader => false;
	}
}
