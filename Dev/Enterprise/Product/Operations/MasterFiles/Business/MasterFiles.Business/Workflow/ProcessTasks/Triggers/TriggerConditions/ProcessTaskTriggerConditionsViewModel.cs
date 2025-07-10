namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskTriggerConditionsViewModel : TriggerConditionsViewModel
	{
		public ProcessTaskTriggerConditionsViewModel(ProcessTask trigger)
			: base(trigger)
		{
		}

		internal new ProcessTask Trigger
		{
			get { return (ProcessTask)base.Trigger; }
		}

		protected override TriggerConditionsViewModelLookups CreateLookups()
		{
			return new ProcessTaskTriggerConditionsViewModelLookups(this);
		}
	}
}
