using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleCollection : ActiveBusinessObjectCollection<ProcessFieldChangeRule>
	{
		public ProcessFieldChangeRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
