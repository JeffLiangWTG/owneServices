using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.HRM.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(ReviewProposal))]
	class ReviewProposalTest : EnterpriseBusinessObjectTestCase
	{
		protected override DbConnection TestConnection => elevatedConnection ??= Db.NewAdminConnection();
		DbConnection elevatedConnection;

		protected override BusinessObjectFactory NewFactory() => new BusinessObjectFactory(TestConnection);

		protected override void TearDown()
		{
			base.TearDown();
			elevatedConnection?.Dispose();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var node = Factory.NewWithValidTestData<ReviewProcessNode>();
			node.RRN_RPR_ReviewProcess = Factory.NewWithValidTestData<ReviewProcess>().PK;
			node.RRN_GS_Reviewer = Factory.NewWithValidTestData<GlbStaff>().PK;

			var proposal = Factory.New<ReviewProposal>();
			proposal.RRP_GS_Staff = Factory.NewWithValidTestData<GlbStaff>().PK;
			proposal.RRP_RRN_ReviewNode = node.PK;

			return proposal;
		}

		public void TestReallyLongComment()
		{
			var node = Factory.NewWithValidTestData<ReviewProcessNode>();
			node.RRN_RPR_ReviewProcess = Factory.NewWithValidTestData<ReviewProcess>().PK;
			node.RRN_GS_Reviewer = Factory.NewWithValidTestData<GlbStaff>().PK;

			var staffInReview = Factory.NewWithValidTestData<GlbStaff>();
			var proposal = Factory.New<ReviewProposal>();
			proposal.RRP_GS_Staff = staffInReview.PK;
			proposal.RRP_RRN_ReviewNode = node.PK;
			proposal.RRP_Comments = new string('A', 10_000); // 10kb of comments

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory(elevatedConnection) { RefreshEnabled = false };
			var proposalInAnotherFactory = anotherFactory.Load<ReviewProposal>(new ZQuery(ReviewProposalSchema.RRP_GS_Staff, staffInReview.PK)).Single();
			AssertEquals(proposalInAnotherFactory.RRP_Comments.Length, 10_000);
		}
	}
}
