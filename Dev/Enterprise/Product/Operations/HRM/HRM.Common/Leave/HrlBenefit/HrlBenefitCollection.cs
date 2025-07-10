using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlBenefitCollection : ActiveBusinessObjectCollection<HrlBenefit>
	{
		public HrlBenefitCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
