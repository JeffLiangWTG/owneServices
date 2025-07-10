using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class JobCompletionTriggerActionCollection : ActiveBusinessObjectCollection<ProcessTaskNotification>
	{
		public JobCompletionTriggerActionCollection(ProcessJobTriggerLink trigger)
			: base(trigger.Factory, new AdhocCollectionRelationship(typeof(JobCompletionTriggerAction)))
		{
			foreach (var triggerAction in trigger.TemplateTrigger.TriggerActions)
			{
				var clone = (JobCompletionTriggerAction)triggerAction.Clone(new BusinessObjectCloneArgs(new[] { ProcessTaskNotificationSchema.Constants.PQ_P9T_Trigger }, typeof(JobCompletionTriggerAction)));

				clone.PQ_EmailText = triggerAction.PQ_EmailText; // Because it's suppressed in ProcessTaskNotification for some reason...
				clone.JobVersionOfTemplateTrigger = trigger;

				Add(clone);
			}

			SetReadOnlyIncludingChildren(true);
		}

		protected override bool AllowNew => false;
	}
}
