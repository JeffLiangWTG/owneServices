using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public sealed class TemplateProcessTaskCollection : ProcessTaskCollection
	{
		public TemplateProcessTaskCollection(ProcessTaskTemplate template)
			: base(template)
		{
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			var query = new ZQuery(alternativeAdditionalFilter);
			query.IncludeBlob(Enterprise.ZArchitecture.Schema.ProcessTasksSchema.P9_Notes); // Templates are usually loaded to be clonned
			base.Load(query);
			Sort(delegate(ProcessTask x, ProcessTask y)
			{
				int result = x.IsMilestone.CompareTo(y.IsMilestone);
				if (result == 0)
				{
					result = x.P9_Sequence - y.P9_Sequence;
				}
				return result;
			});
		}

		protected internal override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask task, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, task, defaultAssignedStaff);
			if (task != null && task.IsTask && Parent is ProcessTaskTemplate t && t.GlobalTemplate && !t.IsCloningTasks)
			{
				task.P9_ShareTasksForAllCompanies = true;
			}
		}

		#region Implementation

		public new TemplateProcessTask this[int index]
		{
			get { return (TemplateProcessTask)Elements[index]; }
		}

		public new TemplateProcessTask AddNew()
		{
			return (TemplateProcessTask)base.AddNew();
		}

		#endregion
	}
}
