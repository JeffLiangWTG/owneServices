using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemProcessTask : ProcessTasks
	{
		public WorkItemProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public override void Delete()
		{
			var parent = Parent;

			base.Delete();

			if (parent != null)
			{
				parent.OnTaskDeleted();
			}
		}

		#endregion

		#region Property Overrides

		public override ZString P9_Status
		{
			get => base.P9_Status;
			set
			{
				var oldValue = P9_Status;

				base.P9_Status = value;

				if (oldValue != value && !IsDeleted && !IsDeleting)
				{
					var parent = Parent;
					if (parent != null && parent.WorkflowItems.Contains(this))
					{
						parent.OnTaskStatusChanged();
					}
				}
			}
		}

		#endregion

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WorkItem; }
		}

		protected override Type ParentType
		{
			get { return typeof(WorkItem); }
		}

		public new WorkItem Parent
		{
			get { return (WorkItem)base.Parent; }
		}

		#endregion
	}
}
