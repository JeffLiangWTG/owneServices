using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkPivot : AutoProcessTaskIterationLinkPivot, IProcessTaskIterationLinkPivot
	{
		public ProcessTaskIterationLinkPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Iteration))]
		public override ZGuid P9P_P9I_Iteration
		{
			get => base.P9P_P9I_Iteration;
			set => base.P9P_P9I_Iteration = value;
		}

		#endregion

		#region Related Business Objects

		IProcessTaskIterationLink IProcessTaskIterationLinkPivot.Iteration => Iteration;

		public ProcessTaskIterationLink Iteration => Factory.Load<ProcessTaskIterationLink>(P9P_P9I_Iteration);

		#endregion

		#region Business Object Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[]
			{
				ProcessTaskIterationLinkPivotSchema.Constants.P9P_P9_Task,
			});

			return base.CloneInternal(args);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of an error reporter message")]
		public override void OnSaving()
		{
			if (!P9P_ParentId.IsValid || string.IsNullOrEmpty(P9P_ParentTableCode))
			{
				var task = Task;
				P9P_ParentId = task?.P9_ParentID ?? ZGuid.Empty;
				P9P_ParentTableCode = task?.P9_ParentTableCode ?? ZString.Empty;

				var taskIsNotNullString = task == null ? string.Empty : "not ";
				ErrorReporter.ReportOnce("TaskPivotParentDetailsEmpty", $@"Tried to save ProcessTaskIterationLinkPivot with missing ParentId and/or ParentTableCode.
Details have been filled in from the pivot's task but this shouldn't be necessary.
Please investigate the stack trace and ensure that these fields are set before saving.
Task is {taskIsNotNullString}null.");
			}

			base.OnSaving();
		}

		#endregion
	}
}
