using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class JobCompletionTriggerAction : ProcessTaskNotification
	{
		public JobCompletionTriggerAction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal IWorkflowTrigger JobVersionOfTemplateTrigger { get; set; }

		protected override IBaseTrigger GetParent()
		{
			return JobVersionOfTemplateTrigger;
		}

		public override bool IsSavedByFactory => false;

		public override void Delete()
		{
			throw new NotSupportedException("This is a non-persisted version of a ProcessTaskNotification and should never be saved.");
		}

		public override void OnSaving()
		{
			throw new NotSupportedException("This is a non-persisted version of a ProcessTaskNotification and should never be saved.");
		}
	}
}
