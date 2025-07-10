using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateProcessTaskLookups : ProcessTasksLookups
	{
		public TemplateProcessTaskLookups(TemplateProcessTask parent)
			: base(parent)
		{
		}

		#region Line Trigger Types

		protected override ZString ParentWorkflowType
		{
			get { return Parent.Parent != null ? Parent.Parent.P0_ProcessType : base.ParentWorkflowType; }
		}

		#endregion

		#region Parent

		new TemplateProcessTask Parent
		{
			get { return (TemplateProcessTask)base.Parent; }
		}

		#endregion
	}
}
