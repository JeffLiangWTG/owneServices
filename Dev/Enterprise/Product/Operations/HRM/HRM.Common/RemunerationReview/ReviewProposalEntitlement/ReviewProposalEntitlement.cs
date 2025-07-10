using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.HRM.Common
{
	public class ReviewProposalEntitlement : AutoReviewProposalEntitlement
	{
		public ReviewProposalEntitlement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ReviewProposal Proposal => Factory.Load<ReviewProposal>(RRE_RRP_Proposal);

		[RelatedBusinessObject(nameof(Proposal))]
		public override ZGuid RRE_RRP_Proposal { get => base.RRE_RRP_Proposal; set => base.RRE_RRP_Proposal = value; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RRE_Value = 3.50m;

			if (!RRE_RRP_Proposal.IsValid)
			{
				RRE_RRP_Proposal = Factory.NewWithValidTestData<ReviewProposal>().PK;
			}
		}
#endif
	}
}
