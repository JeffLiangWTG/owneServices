using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class ProcessCompanyLinkRuleCollection : ActiveBusinessObjectCollection<ProcessCompanyLinkRule>
	{
		public ProcessCompanyLinkRuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
