using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class CompletionTriggerActionCollection : CompletionTriggerActionBaseCollection<ProcessTaskNotification, ProcessTemplateTrigger>
	{
		public CompletionTriggerActionCollection(ProcessTemplateTrigger trigger)
			: base(trigger, ProcessTaskNotificationSchema.PQ_P9T_Trigger)
		{
		}
	}
}
