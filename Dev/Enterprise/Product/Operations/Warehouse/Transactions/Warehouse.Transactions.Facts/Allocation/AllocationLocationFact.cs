using CargoWise.Common;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class AllocationLocationFact : LocationFact, IAllocationLocationFact
	{
		public AllocationLocationFact(IWhsLocation location)
			: base(CheckLocationNotNull(location).PK.ToGuid(),
				  location.WLV_LocationTypeCode,
				  location.WLV_LocationClass,
				  location.PickingAreaName,
				  location.WLV_RowName,
				  location.WLV_Column,
				  location.WLV_Level,
				  location.WLV_Tray,
				  location.WLV_LocationStatus,
				  location.WLV_LocationClass.EqualsIgnoringCase(LocationClasses.Codes.FIX),
				  location.WLV_LocationClass.EqualsIgnoringCase(LocationClasses.Codes.DPF))
		{
			PickPathSequence = location.WLV_PickPathSequence;
			RowPathSequence = location.RowPathSequence;

			IsBonded = location.WLV_PickingAreaType.EqualsIgnoringCase(AreaTypes.Codes.Bonded);
			IsExcise = location.WLV_PickingAreaType.EqualsIgnoringCase(AreaTypes.Codes.Excise);
			IsInwardsProcessing = location.WLV_PickingAreaType.EqualsIgnoringCase(AreaTypes.Codes.InwardProcessing);
		}

		static IWhsLocation CheckLocationNotNull(IWhsLocation location) => Argument.NotNull(location, nameof(location));

		public int PickPathSequence { get; }

		public int RowPathSequence { get; }

		public bool IsAllocatedOnThisPick { get; set; }

		public bool IsBonded { get; }

		public bool IsExcise { get; }

		public bool IsInwardsProcessing { get; }
	}
}
