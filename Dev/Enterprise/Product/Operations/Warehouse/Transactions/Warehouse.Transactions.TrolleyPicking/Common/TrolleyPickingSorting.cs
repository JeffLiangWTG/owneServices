using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	#region SortPickLinesForTrolleyPicking

	public class SortPickLinesForTrolleyPicking : SortPickLinesForPickingSlip
	{
		protected override IEnumerable<IComparer<WhsPickLine>> GetElementaryComparers()
		{
			var result = base.GetElementaryComparers().ToList();
			result.AddRange(new IComparer<WhsPickLine>[]
			{
				new ValueComparer(line =>
				{
					var factory = line.Factory;
					var divot = factory.LoadTop1<PkgPackageItemDivot>(new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, line.PK));
					var pkgPakage = divot?.ParentPackage;
					var trolleySlot = pkgPakage != null ? factory.LoadTop1<WhsPickTrolleySlot>(new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, pkgPakage.PK)) : null;

					return trolleySlot?.WTS_SlotNumber ?? ZShort.Zero;
				}),
				new ValueComparer(line => line.PK), // so we can use this in sorted dictionaries. 
			});

			return result;
		}
	}

	#endregion
}
