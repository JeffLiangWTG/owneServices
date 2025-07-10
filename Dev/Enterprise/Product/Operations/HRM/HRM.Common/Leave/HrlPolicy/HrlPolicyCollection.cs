using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlPolicyCollection : ActiveBusinessObjectCollection<HrlPolicy>
	{
		public HrlPolicyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
