using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CusRefPacksCollection : BusinessObjectCollection<CusRefPacks>
	{
		public CusRefPacksCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
