
using CargoWise.EntityFramework;

namespace Enterprise.eManifest.Business
{
	public class ELoadListCollection : BusinessObjectCollection<ELoadList>
	{
		public ELoadListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
