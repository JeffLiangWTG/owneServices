using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleCollection : ActiveBusinessObjectCollection<USCRule>
	{
		public USCRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
