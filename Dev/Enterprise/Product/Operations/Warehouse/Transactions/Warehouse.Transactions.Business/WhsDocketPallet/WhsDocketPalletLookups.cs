
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketPalletLookups : AutoWhsDocketPalletLookups
	{
		public WhsDocketPalletLookups(AutoWhsDocketPallet parent)
			: base(parent)
		{
		}

		public PalletType PalletTypes
		{
			get { return new PalletType(); }
		}
	}
}
