using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public class ProcessTemplateTriggerConditionsViewModelValidation : TriggerConditionsViewModelValidation
	{
		public ProcessTemplateTriggerConditionsViewModelValidation(TriggerConditionsViewModel parent)
			: base(parent)
		{
		}

		protected override void CheckTriggerFieldName()
		{
			if (!Parent.TriggerFieldName.IsEmpty)
			{
				Parent.TriggerFieldNameInfo.AddError(Res.GetString("f1be0110-5d26-4720-bbd8-f372d439d2a1", "Field Change triggers are not currently supported on Universal Templates."));
			}
		}
	}
}
