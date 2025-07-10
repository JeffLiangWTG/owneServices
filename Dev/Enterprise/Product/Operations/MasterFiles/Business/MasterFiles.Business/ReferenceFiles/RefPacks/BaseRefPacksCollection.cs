using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for BaseRefPacksCollection.
	/// </summary>
	public class BaseRefPacksCollection : ActiveBusinessObjectCollection<BaseRefPacks>
	{
		public BaseRefPacksCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
