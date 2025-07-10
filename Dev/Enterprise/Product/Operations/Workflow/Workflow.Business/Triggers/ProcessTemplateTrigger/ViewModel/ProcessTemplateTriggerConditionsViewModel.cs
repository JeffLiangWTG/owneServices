using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public class ProcessTemplateTriggerConditionsViewModel : TriggerConditionsViewModel
	{
		public ProcessTemplateTriggerConditionsViewModel(IBaseTrigger trigger)
			: base(trigger)
		{
		}

		public override TriggerConditionsViewModelValidation GetNewValidation()
		{
			return new ProcessTemplateTriggerConditionsViewModelValidation(this);
		}

		[BusinessObjectTestExclude]//ProcessTemplateTriggers have readonly TriggerContextCode
		public override ZString TriggerContextCode
		{
			get => base.TriggerContextCode;
			set => base.TriggerContextCode = value;
		}
	}
}
