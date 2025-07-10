using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessNodeCollection : ActiveBusinessObjectCollection<ReviewProcessNode>
	{
		public ReviewProcessNodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
