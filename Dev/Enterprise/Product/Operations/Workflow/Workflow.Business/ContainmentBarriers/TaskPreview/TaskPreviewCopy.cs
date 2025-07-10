using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public class TaskPreviewCopy : NonPersistentBusinessObject
	{
		public TaskPreviewCopy(ProcessTask task, ContainmentBarrierViewModel viewModel)
			: base(Argument.NotNull(task, nameof(task)).Factory)
		{
			Argument.NotNull(viewModel, nameof(viewModel));

			Task = task;
			ViewModel = viewModel;
		}

		public ProcessTask Task { get; }

		internal ContainmentBarrierViewModel ViewModel { get; }

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IncludeInIteration = ZBool.True;
		}

		#endregion

		#region Properties

		#region IncludeInIteration

		[ResourceStringData("TaskPreviewCopy.IncludeInIteration", Caption = "Include In Iteration", ShortCaption = "Included", FullDescription = "Include this task in the Quality Iteration.")]
		public ZBool IncludeInIteration
		{
			get { return includeInIteration; }
			set
			{
				SetNonPersistentPropertyValue(IncludeInIterationInfo, ref includeInIteration, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIncludeInIteration();
				}
			}
		}

		ZBool includeInIteration;

		public ZPropertyInfo IncludeInIterationInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInIteration)); }
		}

		#endregion

		#region Task Properties

		[ResourceStringData("TaskPreviewCopy.TaskName", Caption = "Task Name")]
		public ZString TaskName
		{
			get { return Task.P9_Description; }
		}

		[ResourceStringData("TaskPreviewCopy.Sequence", Caption = "Sequence", ShortCaption = "Seq.")]
		public ZInt Sequence
		{
			get { return Task.P9_Sequence; }
		}

		[ResourceStringData("TaskPreviewCopy.ResourceName", Caption = "Resource Name")]
		public ZString ResourceName
		{
			get { return Task.StaffName; }
		}

		[ResourceStringData("TaskPreviewCopy.WorkflowName", Caption = "Workflow Name")]
		public ZString WorkflowName
		{
			get { return Task.ProcessHeader != null ? Task.ProcessHeader.FH_CompletionStatement : ZString.Empty; }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public TaskPreviewCopyValidation Validation
		{
			get { return GetNewValidation(); }
		}

		public TaskPreviewCopyValidation GetNewValidation()
		{
			return new TaskPreviewCopyValidation(this);
		}

		#endregion
	}
}
