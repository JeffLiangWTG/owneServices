using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions.HR;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.HRM.Common;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	[UseSnapshotProtection(skipTransaction: true)]
	[TestedType(typeof(ReviewProcessNodeCreationServiceTask))]
	class ReviewProcessNodeCreationServiceTaskTest : ServiceTaskTestCase<ReviewProcessNodeCreationServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
			=> new[] { new TaskNudgeInformationForTest("ReviewProcess", "Review Processes Ready to Build", new[] { "RPR_Status=BLD" }) };

		public void TestBasicTinyHierarchy()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";

			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(staff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), null, null, false)
			});

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, StubAllocations(staff)) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var createdNodes = LoadNodes(process);
			AssertEquals("One manager reviewing one staff member with no moderation - only one job", 1, createdNodes.Length);

			var node = createdNodes[0];
			AssertEquals(manager.PK, node.RRN_GS_Reviewer);
			AssertEquals(node.Proposals.Count, 1);

			var proposal = node.Proposals.Single();
			AssertEquals(staff.PK, proposal.RRP_GS_Staff);
			AssertEquals("BAS", proposal.Entitlements.Single().RRE_EntitlementCode);
			AssertEquals(10m, proposal.Entitlements.Single().RRE_BudgetPercent);
			AssertEquals(15m, proposal.Entitlements.Single().RRE_MeritBudgetPercent);

			process.Reload();
			AssertEquals("WRK", process.RPR_Status);
		}

		public void TestReviewAllocations()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var salesStaff = Factory.NewWithValidTestData<GlbStaff>();
			var devStaff = Factory.NewWithValidTestData<GlbStaff>();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";
			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(salesStaff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(devStaff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), null, null, false),
			});

			var allocationsProvider = new Mock<IReviewAllocationProvider>();
			allocationsProvider.Setup(a => a.LoadBudgetAllocations(process.PK.ToGuid())).Returns(new[]
			{
				new BudgetAllocation { StaffPK = salesStaff.PK.ToGuid(), EntitlementCode = "BAS", BudgetPercent = 1, MeritBudgetPercent = 11 },
				new BudgetAllocation { StaffPK = salesStaff.PK.ToGuid(), EntitlementCode = "RME", BudgetPercent = 2, MeritBudgetPercent = 12 },
				new BudgetAllocation { StaffPK = salesStaff.PK.ToGuid(), EntitlementCode = "PBO", BudgetPercent = 3, MeritBudgetPercent = 13 },
				new BudgetAllocation { StaffPK = devStaff.PK.ToGuid(), EntitlementCode = "BAS", BudgetPercent = 4, MeritBudgetPercent = 14 },
				new BudgetAllocation { StaffPK = devStaff.PK.ToGuid(), EntitlementCode = "RME", BudgetPercent = 5, MeritBudgetPercent = 15 },
			});

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, allocationsProvider.Object) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var proposals = LoadNodes(process).Single().Proposals.ToDictionary(p => p.RRP_GS_Staff, p => p.Entitlements.ToDictionary(e => e.RRE_EntitlementCode.ToString()));
			AssertContainsExactElementsInAnyOrder(new[] { salesStaff.PK, devStaff.PK }, proposals.Keys);
			AssertContainsExactElementsInAnyOrder(new[] { "BAS", "RME", "PBO" }, proposals[salesStaff.PK].Keys);
			AssertContainsExactElementsInAnyOrder(new[] { "BAS", "RME", }, proposals[devStaff.PK].Keys);

			CombineAssertions(() =>
			{
				AssertEquals(proposals[salesStaff.PK]["BAS"].RRE_BudgetPercent, 1m);
				AssertEquals(proposals[salesStaff.PK]["RME"].RRE_BudgetPercent, 2m);
				AssertEquals(proposals[salesStaff.PK]["PBO"].RRE_BudgetPercent, 3m);
				AssertEquals(proposals[devStaff.PK]["BAS"].RRE_BudgetPercent, 4m);
				AssertEquals(proposals[devStaff.PK]["RME"].RRE_BudgetPercent, 5m);

				AssertEquals(proposals[salesStaff.PK]["BAS"].RRE_MeritBudgetPercent, 11m);
				AssertEquals(proposals[salesStaff.PK]["RME"].RRE_MeritBudgetPercent, 12m);
				AssertEquals(proposals[salesStaff.PK]["PBO"].RRE_MeritBudgetPercent, 13m);
				AssertEquals(proposals[devStaff.PK]["BAS"].RRE_MeritBudgetPercent, 14m);
				AssertEquals(proposals[devStaff.PK]["RME"].RRE_MeritBudgetPercent, 15m);
			});
		}

		public void TestNonRemReviewSkipsAllocations()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var salesStaff = Factory.NewWithValidTestData<GlbStaff>();
			var devStaff = Factory.NewWithValidTestData<GlbStaff>();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_Type = "PER";
			process.RPR_ConfigType = ZString.Empty;
			process.RPR_RX_NKCurrency = ZString.Empty;
			process.RPR_S9_EmployeesInReview = Factory.NewWithValidTestData<StmModuleFilter>().PK;
			process.RPR_GC_Company = ZGuid.Empty;
			process.RPR_ExchangeRateEffectiveDate = ZDate.Empty;

			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(salesStaff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(devStaff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), null, null, false),
			});

			var allocationsProvider = new Mock<IReviewAllocationProvider>();
			allocationsProvider.Setup(a => a.LoadBudgetAllocations(It.IsAny<Guid>())).Throws(new InvalidOperationException("Not a rem review!"));

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, allocationsProvider.Object) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var proposals = LoadNodes(process).Single().Proposals.ToDictionary(p => p.RRP_GS_Staff);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), proposals[salesStaff.PK.ToGuid()].Entitlements.Select(e => e.RRE_EntitlementCode.ToString()));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), proposals[devStaff.PK.ToGuid()].Entitlements.Select(e => e.RRE_EntitlementCode.ToString()));
		}

		public void TestBasicSmallHierarchy()
		{
			var moderator = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";
			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(staff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), null, moderator.PK.ToGuid(), false),
				new ReviewHierarchyNode(moderator.PK.ToGuid(), null, null, false),
			});

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, StubAllocations(staff)) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var createdNodes = LoadNodes(process).ToDictionary(n => n.RRN_GS_Reviewer);

			AssertEquals("One manager reviewing one staff member with one moderation - one job to review and one to moderate", 2, createdNodes.Count);

			var proposals = createdNodes[manager.PK].Proposals;
			AssertEquals("Staff for review should have a proposal created", staff.PK, proposals.Single().RRP_GS_Staff);
			AssertEquals("Node should be reviewed by moderator node", createdNodes[moderator.PK].PK, createdNodes[manager.PK].RRN_RRN_Parent);

			proposals = createdNodes[moderator.PK].Proposals;
			AssertEquals("Moderator is not reviewing any salaries themselves, only proposals", 0, proposals.Count);

			AssertWorkflowRelationship(createdNodes[moderator.PK], createdNodes[manager.PK]);
		}

		public void TestBasicSmallHierarchySkipsExistingNodes()
		{
			var moderator = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";

			var moderatorNode = Factory.NewWithValidTestData<ReviewProcessNode>();
			moderatorNode.RRN_GS_Reviewer = moderator.PK;
			moderatorNode.RRN_RPR_ReviewProcess = process.PK;

			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(staff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), null, moderator.PK.ToGuid(), false),
				new ReviewHierarchyNode(moderator.PK.ToGuid(), null, null, false),
			});

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, StubAllocations(staff)) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var createdNodes = LoadNodes(process).ToDictionary(n => n.RRN_GS_Reviewer);

			AssertEquals("One manager reviewing one staff member with one moderation - one job to review and one to moderate", 2, createdNodes.Count);

			var proposals = createdNodes[manager.PK].Proposals;
			AssertEquals("Staff for review should have a proposal created", staff.PK, proposals.Single().RRP_GS_Staff);
			AssertEquals("Node should be reviewed by moderator node", createdNodes[moderator.PK].PK, createdNodes[manager.PK].RRN_RRN_Parent);

			proposals = createdNodes[moderator.PK].Proposals;
			AssertEquals("Moderator is not reviewing any salaries themselves, only proposals", 0, proposals.Count);
		}

		public void TestHierarchyWhereManagersAreInReview()
		{
			var moderator = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";
			Factory.Save();

			var provider = new Mock<IReviewHierarchyProvider>();

			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(new[]
			{
				new ReviewHierarchyNode(staff.PK.ToGuid(), manager.PK.ToGuid(), manager.PK.ToGuid(), true),
				new ReviewHierarchyNode(manager.PK.ToGuid(), moderator.PK.ToGuid(), moderator.PK.ToGuid(), true),
				new ReviewHierarchyNode(moderator.PK.ToGuid(), null, null, false),
			});

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, StubAllocations(staff, manager)) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var createdNodes = LoadNodes(process).ToDictionary(n => n.RRN_GS_Reviewer);

			AssertEquals("One manager reviewing one staff member with one moderation - one job to review and one to moderate", 2, createdNodes.Count);

			var proposals = createdNodes[manager.PK].Proposals;
			AssertEquals("Staff for review should have a proposal created", staff.PK, proposals.Single().RRP_GS_Staff);
			AssertEquals("Node should be reviewed by moderator node", createdNodes[moderator.PK].PK, createdNodes[manager.PK].RRN_RRN_Parent);

			proposals = createdNodes[moderator.PK].Proposals;
			AssertEquals("Manager is in review and should have proposal created", manager.PK, proposals.Single().RRP_GS_Staff);
		}

		public void TestLargerHierarchyWithBatchedSaving()
		{
			var staff = Enumerable.Range(0, 20).Select(_ => Factory.NewWithValidTestData<GlbStaff>()).ToArray();

			var process = Factory.NewWithValidTestData<ReviewProcess>();
			process.RPR_Status = "BLD";
			process.RPR_ConfigType = "D";

			Factory.Save();

			var staffInReviewNode = new ReviewHierarchyNode(staff[0].PK.ToGuid(), staff[1].PK.ToGuid(), staff[1].PK.ToGuid(), true);
			var topManagerNode = new ReviewHierarchyNode(staff.Last().PK.ToGuid(), null, null, false);
			var middleManagerNodes = Enumerable.Range(1, 18).Select(i => new ReviewHierarchyNode(staff[i].PK.ToGuid(), null, staff[i + 1].PK.ToGuid(), false));
			var allNodes = middleManagerNodes.Concat(new[] { staffInReviewNode, topManagerNode });

			var provider = new Mock<IReviewHierarchyProvider>();
			provider.Setup(p => p.DetermineHierarchy(process.PK.ToGuid())).Returns(allNodes);

			var task = new ReviewProcessNodeCreationServiceTask(provider.Object, StubAllocations(allNodes), batchSize: 3) { ServiceLogger = new DummyLogger() };
			task.RunTask();

			var createdNodes = LoadNodes(process);
			AssertEquals("Created one node per person in hierarchy, aside from leaf node", 19, createdNodes.Length);
		}

		void AssertWorkflowRelationship(ReviewProcessNode parent, ReviewProcessNode child)
		{
			var parentJob = ProcessJobHeaderProvider.GetForParentWithoutCreation(parent, parent.Factory) ?? throw new AssertionFailedError("parent lacks workflow");
			var childJob = ProcessJobHeaderProvider.GetForParentWithoutCreation(child, parent.Factory) ?? throw new AssertionFailedError("parent lacks workflow");
			var links = childJob.LinksFromMeToOthers.Cast<IProcessHeaderLink>().Select(j => j.HeaderTo);

			AssertCollectionContains(parentJob, links);
		}

		IReviewAllocationProvider StubAllocations(IEnumerable<BudgetAllocation> allocations)
		{
			var m = new Mock<IReviewAllocationProvider>();
			m.Setup(p => p.LoadBudgetAllocations(It.IsAny<Guid>())).Returns(allocations);
			return m.Object;
		}

		IReviewAllocationProvider StubAllocations(IEnumerable<Guid> staffPks)
			=> StubAllocations(staffPks.Select(s => new BudgetAllocation { BudgetPercent = 10, MeritBudgetPercent = 15, EntitlementCode = "BAS", StaffPK = s }));

		IReviewAllocationProvider StubAllocations(params GlbStaff[] staff)
			=> StubAllocations(staff.Select(s => s.PK.ToGuid()));

		IReviewAllocationProvider StubAllocations(IEnumerable<ReviewHierarchyNode> nodes)
			=> StubAllocations(nodes.Where(n => n.InReview).Select(n => n.Staff));

		protected override void SetUpCore()
		{
			base.SetUpCore();

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "RPN");
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			connection?.Dispose();
		}

		DbConnection connection;
		protected override DbConnection TestConnection
			=> connection ?? (connection = Db.NewAdminConnection());

		protected override BusinessObjectFactory NewFactory()
			=> new BusinessObjectFactory(TestConnection);

		ReviewProcessNode[] LoadNodes(ReviewProcess process)
			=> Factory.Load<ReviewProcessNode>(new ZQuery(ReviewProcessNodeSchema.RRN_RPR_ReviewProcess, process.PK));
	}
}
