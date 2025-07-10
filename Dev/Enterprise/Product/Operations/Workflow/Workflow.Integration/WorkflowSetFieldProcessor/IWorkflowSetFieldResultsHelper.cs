using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowSetFieldResultsHelper
	{
		public bool IsSettingProperty(BusinessObjectFactory factory, ZPropertyInfo info);
	}
}
