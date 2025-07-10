using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateProcessTask : ProcessTask, ITemplateProcessTask
	{
		public TemplateProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override ProcessTasksLookups GetNewLookups()
		{
			return new TemplateProcessTaskLookups(this);
		}

		#endregion

		#region Related Business Objects

		#region Parent

		protected internal override Type ParentType
		{
			get { return typeof(ProcessTaskTemplate); }
		}

		public new ProcessTaskTemplate Parent
		{
			get { return (ProcessTaskTemplate)base.Parent; }
		}

		#endregion

		#endregion

		#region ProcessTask Overrides

		public override ControllerID ParentControllerID => ControllerIDs.ProcessTemplates;

		protected override string WorkflowTypeCore => Parent?.P0_ProcessType ?? ZString.Empty;

		protected override void CancelIfParentIsCancelled()
		{
			// do nothing
		}

		#endregion
	}
}
