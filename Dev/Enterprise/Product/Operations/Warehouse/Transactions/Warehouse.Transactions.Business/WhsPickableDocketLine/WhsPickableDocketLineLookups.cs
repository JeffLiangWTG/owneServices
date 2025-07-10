using CargoWise.Integration;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickableDocketLineLookups : WhsDocketLineLookups
	{
		public WhsPickableDocketLineLookups(WhsPickableDocketLine parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsPickableDocketLine Parent
		{
			get { return (WhsPickableDocketLine)base.Parent; }
		}

		#endregion

		#region PickGroups

		public ICodeDescriptionPairList PickGroups
		{
			get { return WarehouseDataRegistry.Instance.PickGroups.Value; }
		}

		#endregion
	}
}
