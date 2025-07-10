using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleCollection : ActiveBusinessObjectCollection<AccCommissionRule>
	{
		public AccCommissionRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
