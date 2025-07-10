using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.Common
{
	public class ReviewProposal : AutoReviewProposal
	{
		public ReviewProposal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var relationship = new DependentRelationship(this, typeof(ReviewProposalEntitlement), new ZQuery(), ReviewProposalEntitlementSchema.RRE_RRP_Proposal);
			Entitlements = new ActiveBusinessObjectCollection<ReviewProposalEntitlement>(Factory, relationship);
		}

		public IActiveBusinessObjectCollection<ReviewProposalEntitlement> Entitlements { get; }

		public ReviewProcessNode ReviewNode => Factory.Load<ReviewProcessNode>(RRP_RRN_ReviewNode);

		[RelatedBusinessObject(nameof(ReviewNode))]
		public override ZGuid RRP_RRN_ReviewNode { get => base.RRP_RRN_ReviewNode; set => base.RRP_RRN_ReviewNode = value; }

		public GlbStaff Staff => Factory.Load<GlbStaff>(RRP_GS_Staff);

		[RelatedBusinessObject(nameof(Staff))]
		public override ZGuid RRP_GS_Staff { get => base.RRP_GS_Staff; set => base.RRP_GS_Staff = value; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (!RRP_RRN_ReviewNode.IsValid)
			{
				RRP_RRN_ReviewNode = Factory.NewWithValidTestData<ReviewProcessNode>().PK;
			}

			if (!RRP_GS_Staff.IsValid)
			{
				RRP_GS_Staff = Factory.NewWithValidTestData<GlbStaff>().PK;
			}
		}
#endif
	}
}
