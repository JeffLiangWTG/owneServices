
using CargoWise.EntityFramework;

namespace Enterprise.WebCFS.Business
{
	public class SailingCollection : BusinessObjectCollection<Sailing>
	{
		public SailingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
