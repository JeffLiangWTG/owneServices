using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Definitions.HR;
using CargoWise.EntityFramework;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ReviewProcessNodeCreationServiceTask.Code,
	"Create Jobs to Review Staff",
	"HRM",
	typeof(ReviewProcessNodeCreationServiceTask),
	MinimumPeriod = "1day",
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1day"
	)]

[assembly: HostedServiceBusinessObjectBinding(ReviewProcessNodeCreationServiceTask.Code,
	ReviewProcessSchema.Constants.TableName,
	new[] { ReviewProcessSchema.Constants.RPR_Status + "=" + ReviewProcessStatusCodes.Building },
	"Review Processes Ready to Build"
)]

namespace Enterprise.HRM.Common.ServiceTasks
{
	public class ReviewProcessNodeCreationServiceTask : ServiceProviderImpl
	{
		public const string Code = "RPN";

		readonly IReviewHierarchyProvider hierarchyProvider;
		readonly IReviewAllocationProvider allocationProvider;
		readonly int batchSize;

		public ReviewProcessNodeCreationServiceTask()
			: this(new HierarchyProvider(), new ReviewAllocationProvider()) { }

		public ReviewProcessNodeCreationServiceTask(IReviewHierarchyProvider hierarchyProvider, IReviewAllocationProvider allocationProvider, int batchSize = 50)
		{
			this.hierarchyProvider = hierarchyProvider ?? throw new ArgumentNullException(nameof(hierarchyProvider));
			this.allocationProvider = allocationProvider ?? throw new ArgumentNullException(nameof(allocationProvider));
			this.batchSize = batchSize;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Logger strings")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			using var hrmConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName);
			var factory = new BusinessObjectFactory(hrmConnection);
			var process = factory.LoadTop1<ReviewProcess>(new ZQuery(ReviewProcessSchema.RPR_Status, ReviewProcessStatusCodes.Building));

			if (process == null)
			{
				ServiceLogger.Information("Found no ready review process.");
				return;
			}

			ServiceLogger.Information("Found " + process.RPR_Name);

			var allocations = process.RPR_Type == ReviewProcessTypes.Remuneration
				? allocationProvider.LoadBudgetAllocations(process.PK.ToGuid()).ToLookup(a => a.StaffPK)
				: Array.Empty<BudgetAllocation>().ToLookup(a => a.StaffPK);

			var completedNodes = LoadExistingNodes(factory, process).ToDictionary(n => n.RRN_GS_Reviewer.ToGuid());
			var tree = BuildTreeAndReturnRoots(hierarchyProvider.DetermineHierarchy(process.PK.ToGuid()));

			var nodesToCreate = new Queue<ReviewNode>(tree);
			var nodesCreatedSinceLastSave = 0;
			while (nodesToCreate.Count > 0)
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();

				var node = nodesToCreate.Dequeue();
				node.Children.ForEach(n => nodesToCreate.Enqueue(n));

				var isLeafNode = (node.Children.Count == 0) && (node.Proposals.Count == 0);
				if (isLeafNode)
				{
					continue;
				}

				if (completedNodes.ContainsKey(node.Reviewer))
				{
					continue;
				}

				var bizo = factory.New<ReviewProcessNode>();
				bizo.RRN_GS_Reviewer = node.Reviewer;
				bizo.RRN_RPR_ReviewProcess = process.PK;

				foreach (var staffToReview in node.Proposals)
				{
					var proposal = factory.New<ReviewProposal>();
					proposal.RRP_GS_Staff = staffToReview;
					proposal.RRP_RRN_ReviewNode = bizo.PK;

					foreach (var allocation in allocations[staffToReview])
					{
						var entitlement = factory.New<ReviewProposalEntitlement>();
						entitlement.RRE_RRP_Proposal = proposal.PK;
						entitlement.RRE_EntitlementCode = allocation.EntitlementCode;
						entitlement.RRE_BudgetPercent = allocation.BudgetPercent;
						entitlement.RRE_MeritBudgetPercent = allocation.MeritBudgetPercent;
						entitlement.RRE_Value = 0;
					}
				}

				if (node.Moderator != null)
				{
					var parent = completedNodes[node.Moderator.Value];

					bizo.RRN_RRN_Parent = parent.PK;
					CreateWorkflowRelationship(factory, parent, bizo);
				}

				if (++nodesCreatedSinceLastSave >= batchSize)
				{
					factory.Save();
					nodesCreatedSinceLastSave = 0;
				}

				completedNodes[node.Reviewer] = bizo;
			}

			process.RPR_Status = ReviewProcessStatusCodes.Working;
			factory.Save();
		}

		IEnumerable<ReviewProcessNode> LoadExistingNodes(BusinessObjectFactory factory, ReviewProcess process)
			=> factory.Load<ReviewProcessNode>(new ZQuery(ReviewProcessNodeSchema.RRN_RPR_ReviewProcess, process.PK));

		IEnumerable<ReviewNode> BuildTreeAndReturnRoots(IEnumerable<ReviewHierarchyNode> staffNodes)
		{
			var topNodes = new List<ReviewNode>();
			var nodes = new Dictionary<Guid, ReviewNode>();

			foreach (var hierarchyNode in staffNodes)
			{
				var node = GetOrAdd(hierarchyNode.Staff);

				if (hierarchyNode.InReview)
				{
					var reviewer = GetOrAdd(hierarchyNode.Reviewer.Value);
					reviewer.Proposals.Add(hierarchyNode.Staff);
				}

				if (hierarchyNode.Moderator is null)
				{
					topNodes.Add(node);
					continue;
				}

				node.Moderator = hierarchyNode.Moderator;

				var moderator = GetOrAdd(hierarchyNode.Moderator.Value);
				moderator.Children.Add(node);
			}

			return topNodes;

			ReviewNode GetOrAdd(Guid reviewer)
				=> nodes.TryGetValue(reviewer, out var node) ? node : (nodes[reviewer] = new ReviewNode { Reviewer = reviewer });
		}

		static void CreateWorkflowRelationship(BusinessObjectFactory factory, ReviewProcessNode parent, ReviewProcessNode child)
		{
			var parentWorkflow = ProcessJobHeaderProvider.GetForParent(parent, factory);
			var childWorkflow = ProcessJobHeaderProvider.GetForParent(child, factory);

			if (parentWorkflow is null || childWorkflow is null)
			{
				throw new InvalidOperationException("Missing Process Header for review node (is BMS enabled?)");
			}

			childWorkflow.GetOrCreateDependencyLink(parentWorkflow);
		}

		class ReviewNode
		{
			public Guid? Moderator { get; set; }
			public Guid Reviewer { get; set; }

			public List<Guid> Proposals { get; } = new List<Guid>();
			public List<ReviewNode> Children { get; } = new List<ReviewNode>();
		}
	}
}
