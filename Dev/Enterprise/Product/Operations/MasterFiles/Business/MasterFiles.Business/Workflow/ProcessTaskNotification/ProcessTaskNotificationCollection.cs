using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskNotificationCollection : CompletionTriggerActionBaseCollection<ProcessTaskNotification, ProcessTask>
	{
		public ProcessTaskNotificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProcessTaskNotificationCollection(ProcessTask master)
			: base(master, ProcessTaskNotificationSchema.PQ_P9)
		{
			trigger = master;
		}

		readonly ProcessTask trigger;

		// We can add trigger actions if parent trigger is editable
		protected override bool AllowNew => !trigger.ReadOnly;

		protected override void OnAdded(ProcessTaskNotification businessObject)
		{
			if (businessObject is ProcessTaskNotification notification)
			{
				// Avoid unnecessary Factory.Load
				notification.parent_DoNotTouch = trigger;
			}
			base.OnAdded(businessObject);
		}
	}
}
