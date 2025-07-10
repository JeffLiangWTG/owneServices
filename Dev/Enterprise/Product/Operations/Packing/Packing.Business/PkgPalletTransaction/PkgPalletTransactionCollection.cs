using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPalletTransactionCollection : ActiveBusinessObjectCollection<PkgPalletTransaction>
	{
		public PkgPalletTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
