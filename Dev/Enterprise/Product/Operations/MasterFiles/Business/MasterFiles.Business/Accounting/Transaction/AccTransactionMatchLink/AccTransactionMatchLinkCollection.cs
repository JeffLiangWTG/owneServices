using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionMatchLinkCollection : BusinessObjectCollection<AccTransactionMatchLink>
	{
		public AccTransactionMatchLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccTransactionMatchLinkCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
