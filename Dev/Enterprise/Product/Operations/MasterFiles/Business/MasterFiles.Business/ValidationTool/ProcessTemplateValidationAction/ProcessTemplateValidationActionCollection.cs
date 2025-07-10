using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationActionCollection : ActiveBusinessObjectCollection<ProcessTemplateValidationAction>
	{
		public ProcessTemplateValidationActionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ProcessTemplateValidationActionCollection(ProcessTaskTemplate parent) : base(parent.Factory, parent, new ZQuery(), ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate)
		{
		}

		public ProcessTemplateValidationActionCollection(ProcessTemplateValidation parent) : base(parent.Factory, parent, new ZQuery(), ProcessTemplateValidationActionSchema.P0A_P0V_ValidationRule)
		{
		}

		protected override void SetDefaultsForNewElementCore(ProcessTemplateValidationAction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Relationship.Master is ProcessTemplateValidation validationRule)
			{
				newElement.P0A_P0_WorkflowTemplate = validationRule.P0V_P0_WorkflowTemplate;
			}
		}
	}
}
