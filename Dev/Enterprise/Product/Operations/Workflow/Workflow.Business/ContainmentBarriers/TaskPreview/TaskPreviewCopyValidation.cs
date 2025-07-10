using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class TaskPreviewCopyValidation : ZValidation
	{
		public TaskPreviewCopyValidation(TaskPreviewCopy parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly TaskPreviewCopy parent;

		public void ValidateIncludeInIteration()
		{
			ValidateCalculatedProperty(parent.IncludeInIterationInfo);
		}

		protected void CheckIncludeInIteration()
		{
			if (!parent.IncludeInIteration)
			{
				if (parent.ViewModel.IterateFromTask == parent.Task)
				{
					parent.IncludeInIterationInfo.AddError(Res.GetString("4fcd947e-2285-4c93-ac93-46780fdf33f8", "The Iterate From Task must be included in the Quality Iteration."));
				}
				else if (parent.ViewModel.ContainmentBarrierTask == parent.Task)
				{
					parent.IncludeInIterationInfo.AddError(Res.GetString("4df9ebad-a075-45d9-bce7-16588b5a153b", "The Quality Containment Barrier task must be included in the Quality Iteration."));
				}
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(TaskPreviewCopy); }
		}

		public override void ValidateAll()
		{
			ValidateIncludeInIteration();
		}
	}
}
