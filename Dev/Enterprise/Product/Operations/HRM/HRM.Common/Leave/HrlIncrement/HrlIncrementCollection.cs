using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlIncrementCollection : ActiveBusinessObjectCollection<HrlIncrement>
	{
		public HrlIncrementCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
