using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.HRM.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(ReviewProcessNode))]
	class ReviewProcessNodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocManagerSupportImplementation()
		{
			var reviewProcessNode = Factory.New<ReviewProcessNode>();
			AssertNotNull(reviewProcessNode.DocManagerInfo);
			AssertEquals(reviewProcessNode.DocManagerInfo.DocManagerCode, Core.Constants.DocManagerCodes.ReviewProcessNode);
		}

		[UseSnapshotProtection]
		public override void TestCanExportEDocViaUniversalXml()
			=>	base.TestCanExportEDocViaUniversalXml();

		[UseSnapshotProtection]
		public void TestConversation()
		{
			var provider = (IConversationProvider)node;
			Factory.Save();

			var conversation = provider.eConversation;
			var expectedConversation = JobConversation.GetConversation(node);

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestConversation_NotSaved()
		{
			var provider = (IConversationProvider)node;

			AssertNull(provider.eConversation);
		}

		[UseSnapshotProtection]
		public void TestConversation_AlreadyExists()
		{
			var provider = (IConversationProvider)node;
			Factory.Save();

			var expectedConversation = JobConversation.GetOrCreate(node);

			var conversation = provider.eConversation;

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestParentModule()
		{
			var provider = (IConversationProvider)node;

			AssertEquals(ModuleIDs.ReviewProcessNode, provider.ParentModule);
		}

		public void TestParentController()
		{
			var provider = (IConversationProvider)node;

			AssertEquals(ControllerIDs.ReviewProcessNode, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			var provider = (IConversationProvider)node;

			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			var provider = (IConversationProvider)node;

			AssertEquals(provider.SendEmailNotificationsOnSave, false);
		}

		public void TestEmailSubjectContentOverride()
		{
			var provider = (IConversationProvider)node;

			AssertNull(provider.EmailSubjectContentOverride);
		}

		public void TestGetAdditionalParticipants_StaffSubscriber()
		{
			var reviewer = node.Reviewer;
			var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { reviewer });

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = contact.TablePrefix;
			participant.ParentKey = contact.PK.ToString();

			var expectedResult = new List<IConversationParticipant>() { reviewer };

			var results = node.GetAdditionalParticipants(subscribedParticipants, participant);

			AssertCollectionNotContains(null, results);

			AssertSequencesEqual(expectedResult, node.GetAdditionalParticipants(subscribedParticipants, participant));
		}

		public void TestReadonly()
		{
			Assert(node.RRN_RRN_ParentInfo.ReadOnly);
			Assert(node.RRN_GS_ReviewerInfo.ReadOnly);
			Assert(node.RRN_RPR_ReviewProcessInfo.ReadOnly);
			Assert(node.RRN_StatusInfo.ReadOnly);
		}

		public void TestHumanReadableName()
		{
			var node = Factory.New<ReviewProcessNode>();
			AssertEquals("Review", node.HumanReadableName);

			var reviewProcess = Factory.New<ReviewProcess>();
			reviewProcess.RPR_Name = "2024 Promotions Review";
			node.RRN_RPR_ReviewProcess = reviewProcess.PK;
			AssertEquals("2024 Promotions Review", node.HumanReadableName);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			node.RRN_GS_Reviewer = staff.PK;
			AssertEquals("2024 Promotions Review - ABC", node.HumanReadableName);

			node = Factory.New<ReviewProcessNode>();
			node.RRN_GS_Reviewer = staff.PK;
			AssertEquals("Review - ABC", node.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			node = Factory.NewWithValidTestData<ReviewProcessNode>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			escalatedConnection?.Dispose();
		}

		protected override DbConnection TestConnection => escalatedConnection ??= Db.NewAdminConnection();
		AdminConnection escalatedConnection;

		protected override BusinessObjectFactory NewFactory()
			=> new BusinessObjectFactory(TestConnection);

		ReviewProcessNode node;
		protected override BusinessObject GetNewBusinessObject()
			=> Factory.NewWithValidTestData<ReviewProcessNode>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var node = Factory.NewWithValidTestData<ReviewProcessNode>();
			node.RRN_RPR_ReviewProcess = Factory.NewWithValidTestData<ReviewProcess>().PK;
			node.RRN_GS_Reviewer = Factory.NewWithValidTestData<GlbStaff>().PK;

			return node;
		}
	}

	[TestedType(typeof(ReviewProcessNode))]
	class ReviewProcessNodeWorkflowProviderTest : WorkflowProviderTest<ReviewProcessNode, ReviewProcessNodeProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ReviewProcessNodeWorkflowDescriptorCode;
	}
}
