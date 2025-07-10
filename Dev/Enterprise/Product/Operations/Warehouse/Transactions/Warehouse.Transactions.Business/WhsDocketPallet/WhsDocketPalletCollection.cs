using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketPalletCollection : ActiveBusinessObjectCollection<WhsDocketPallet>
	{
		public WhsDocketPalletCollection(WhsDocket master, BusinessObjectFactory factory)
			: base(factory, master)
		{
		}

		protected override bool AllowNew => base.AllowNew && !((WhsDocket)Relationship.Master).IsCreatedFromPickByBOM;
	}
}
