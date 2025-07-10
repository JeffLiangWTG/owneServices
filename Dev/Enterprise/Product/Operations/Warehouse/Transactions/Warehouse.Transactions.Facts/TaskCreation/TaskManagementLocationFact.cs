using CargoWise.Common;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementLocationFact : LocationFact, ITaskManagementLocationFact
	{
		public TaskManagementLocationFact(IWhsLocation location)
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
			PutawayPathSequence = location.WLV_PutawayPathSequence;
			PickPathSequence = location.WLV_PickPathSequence;
			RowPathSequence = location.RowPathSequence;
		}

		static IWhsLocation CheckLocationNotNull(IWhsLocation location) => Argument.NotNull(location, nameof(location));

		public int PutawayPathSequence { get; }

		public int PickPathSequence { get; }

		public int RowPathSequence { get; }
	}
}
