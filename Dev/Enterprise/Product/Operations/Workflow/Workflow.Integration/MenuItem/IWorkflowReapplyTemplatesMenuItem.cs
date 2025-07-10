using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowReapplyTemplatesMenuItem
	{
		IMenuItem GetReapplyWorkflowTemplateMenuItemForBusinessObjectFrom(BusinessObject businessObject);
	}
}
