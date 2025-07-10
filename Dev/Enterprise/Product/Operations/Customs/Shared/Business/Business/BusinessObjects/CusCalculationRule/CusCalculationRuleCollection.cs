using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusCalculationRuleCollection<T> : ActiveBusinessObjectCollection<T> where T : CusCalculationRule
	{
		public CusCalculationRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
