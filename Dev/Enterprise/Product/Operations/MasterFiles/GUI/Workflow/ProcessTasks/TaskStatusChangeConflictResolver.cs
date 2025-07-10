using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class TaskStatusChangeConflictResolver : ITaskStatusChangeConflictResolver
	{
#if DEBUG
		bool TryGetResolution_ForTest(out TaskStatusChangeConflictResolution resolution)
		{
			if (Globals.IsTest)
			{
				if (NextDialogResultToReturnForTest.IsOverriden)
				{
					resolution = CreateResolution(NextDialogResultToReturnForTest.Value, NextTaskToStartForTest.Value);
				}
				else
				{
					resolution = TaskStatusChangeConflictResolution.Continue();
				}

				return true;
			}
			else
			{
				resolution = null;
				return false;
			}
		}

		internal static readonly Overridable<ProcessTask> NextTaskToStartForTest = new Overridable<ProcessTask>();
		internal static readonly Overridable<DialogResult> NextDialogResultToReturnForTest = new Overridable<DialogResult>(DialogResult.None);
#endif

		public ITaskStatusChangeConflictResolution GetExistingWorkTaskExists(IProcessTask task)
		{
			if (Db.Connection.IsInTransactionOtherThanTransactionedTestCase)
			{
				return TaskStatusChangeConflictResolution.Continue();
			}

#if DEBUG
			if (TryGetResolution_ForTest(out var resolution))
			{
				return resolution;
			}
#endif

			using (var form = new ExistingWorkingTaskForm((ProcessTask)task))
			{
				return CreateResolution(ZFormModaliser.ShowDialogWithoutDispose(form), form.TaskToStart);
			}
		}

		static TaskStatusChangeConflictResolution CreateResolution(DialogResult dialogResult, ProcessTask alternativeTask)
		{
			if (dialogResult == DialogResult.Yes)
			{
				return TaskStatusChangeConflictResolution.Continue();
			}
			else
			{
				return TaskStatusChangeConflictResolution.SelectAlternative(alternativeTask);
			}
		}
	}
}
