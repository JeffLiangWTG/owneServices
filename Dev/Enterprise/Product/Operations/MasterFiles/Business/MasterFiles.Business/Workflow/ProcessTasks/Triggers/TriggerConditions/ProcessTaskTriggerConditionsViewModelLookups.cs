namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskTriggerConditionsViewModelLookups : TriggerConditionsViewModelLookups
	{
		public ProcessTaskTriggerConditionsViewModelLookups(ProcessTaskTriggerConditionsViewModel viewModel)
			: base(viewModel)
		{
		}

		protected new ProcessTask Trigger
		{
			get { return (ProcessTask)base.Trigger; }
		}
	}
}
