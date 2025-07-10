using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskCompletionStatementCollectionView : ProcessTaskCollectionView
	{
		public ProcessTaskCompletionStatementCollectionView(ProcessTaskTemplate template)
			: base(template.WorkflowItems)
		{
			this.template = template;
		}

		readonly ProcessTaskTemplate template;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return base.IsThisPartOfTheCollection(element) && ((ProcessTask)element).IsCompletionStatement;
		}

		protected override void AddNewTaskCore(ProcessTask task)
		{
			using (task.GetValidationSuspender())
			{
				var taskType = ProcessTask.GetCompletionStatementTaskType(template.P0_ProcessType);
				SetDefaultsOnNewCompletionStatement(this, task, taskType);
				base.AddNewTaskCore(task);
			}
		}

		public static void SetDefaultsOnNewCompletionStatement(ProcessTaskCollectionView collection, ProcessTask newCompletionStatement, string taskType)
		{
			if (!string.IsNullOrEmpty(taskType))
			{
				newCompletionStatement.P9_Type = taskType;
				newCompletionStatement.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

				var lastTask = collection.Cast<ProcessTask>().Except(newCompletionStatement).MaxBySafe(t => t.P9_Sequence);
				if (lastTask == null || lastTask.P9_Sequence < ProcessTask.CompletionTaskSequenceStart)
				{
					newCompletionStatement.P9_Sequence = ProcessTask.CompletionTaskSequenceStart + 1;
				}
			}
		}
	}
}
