using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTaskFactory : IWhsTaskFactory
	{
		public WhsTaskFactory(IProcessHeaderUniqueCompletionStatementGenerator completionStatementGenerator)
		{
			CompletionStatementGenerator = Argument.NotNull(completionStatementGenerator, nameof(completionStatementGenerator));
		}

		IProcessHeaderUniqueCompletionStatementGenerator CompletionStatementGenerator { get; }

		public ProcessTask CreateTask(
			IWorkflowProvider workflowProvider,
			ZString formflowType,
			ZString workflowName,
			ZString taskName,
			ZString staffCode,
			ZShort rawNudge,
			ZString capabilityCode,
			ZGuid releaseGroupPk,
			string taskType = Core.Constants.Workflow.UndefinedTaskType)
		{
			Argument.NotNull(workflowProvider, nameof(workflowProvider));

			var workflow = workflowProvider.Workflows.AddNew();
			var factory = workflow.Factory;
			var parentWorkflow = ProcessJobHeaderProvider.GetForParent(workflowProvider, factory)
				?? throw new InvalidOperationException("Failed to create or load job level workflow. May not be filtering jobs where Buffer Management is not enabled.");
			workflow.FH_FH_ParentHeader = parentWorkflow.PK;
			workflow.FH_CompletionStatement = CompletionStatementGenerator.GetUniqueCompletionStatement(workflowProvider.Workflows, workflowName);
			workflow.FH_GG_ReleaseGroup = releaseGroupPk;
			workflow.FH_VoteUpDownAmount = rawNudge;

			var capability = factory.LoadFromNaturalKey<GlbCapability>(GlbCapabilitySchema.G4_Code, capabilityCode);
			var tasks = workflowProvider.WorkflowItems;
			var newSequence = (((long)tasks.Where(t => t.IsTask).MaxOrDefault(t => t.P9_Sequence) / 100) + 1) * 100;
			var task = tasks.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = taskType;
			task.P9_FormFlowType = formflowType;
			task.P9_Sequence = newSequence <= int.MaxValue ? (int)newSequence : 100;
			task.P9_Description = taskName;
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_G4_RequiredCapability = capability?.PK ?? ZGuid.Empty;

			return task;
		}
	}
}
