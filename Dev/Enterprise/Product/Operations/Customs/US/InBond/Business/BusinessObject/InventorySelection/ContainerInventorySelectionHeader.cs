using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.US.InBond.Business
{
	public class ContainerInventorySelectionHeader : MoveHeaderInventorySelectionHeader
	{
		public ContainerInventorySelectionHeader(CusInBondContainer container)
			: base(container.MoveHeader)
		{
			this.container = container;
		}

		protected override FilterBusinessObjectDefaults GetFilterDefaultsCore()
		{
			var result = base.GetFilterDefaultsCore();
			var warehouseEntryNumbers = container.Commodities.OfType<CusInBondCargoDesc>().Select(x => x.BY_WarehouseEntryNumber).Where(x => !x.IsEmpty).Distinct().Take(2).ToArray();
			if (warehouseEntryNumbers.Length == 1)
			{
				result.Add(new FilterBusinessObjectDefault("Customs Entry Key", "Property", warehouseEntryNumbers[0]));
			}
			return result;
		}

		protected override CusInBondContainer GetContainer(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			return container;
		}
		readonly CusInBondContainer container;
	}
}
