using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderLineCollection : WhsPickableDocketLineCollection
	{
		public WhsOrderLineCollection(WhsOrder master)
			: base(master)
		{
		}

		protected WhsOrderLineCollection(WhsOrder master, ZQuery filter)
			: base(master, filter)
		{
		}

		public new WhsOrderLine this[int index]
		{
			get { return (WhsOrderLine)(base[index]); }
		}

		public virtual new WhsOrderLine AddNew()
		{
			return (WhsOrderLine)base.AddNew();
		}

		protected override bool AllowNew
		{
			get
			{
				var master = (WhsOrder)Relationship.Master;
				return base.AllowNew && !IsMultiOrderPickOrPickReadyForPlanningOrPlanned(master) && !master.IsPickingFTZCustomsOrderWithPermit && !master.IsLoadingOrLoadedOrDeparted;
			}
		}

		bool IsMultiOrderPickOrPickReadyForPlanningOrPlanned(WhsOrder whsOrder)
		{
			var pick = whsOrder.Pick;
			return pick != null && (pick.IsMultiOrderPick || pick.IsReadyForPlanningOrPlanned);
		}

		// an order can be edited after it is picked if not yet finalised and 'PreventOrderLinesUpdateWhenOrderIsInPicking' registry is not enabled.
		protected override IEnumerable<ZString> AllowNewAdditionValidDocketStatuses => ((WhsOrder)Relationship.Master).AreLinesUpdateDisabledAfterPick
			? Enumerable.Empty<ZString>()
			: new ZString[] { DocketStatus.Codes.AttachedToPick, DocketStatus.Codes.Picking };

		// an order can be edited after it is picked if not yet finalised and 'PreventOrderLinesUpdateWhenOrderIsInPicking' registry is not enabled.
		protected override IEnumerable<ZString> AllowRemoveAdditionValidDocketStatuses => ((WhsOrder)Relationship.Master).AreLinesUpdateDisabledAfterPick
			? Enumerable.Empty<ZString>()
			: new ZString[] { DocketStatus.Codes.AttachedToPick, DocketStatus.Codes.Picking };
	}

	public class WhsOrderLineCollectionWithoutChildLines : WhsOrderLineCollection
	{
		public WhsOrderLineCollectionWithoutChildLines(WhsOrder order)
			: base(order, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, null))
		{
		}
	}

	public class WhsOrderLineWorkCollection : BusinessObjectCollection<WhsOrderLine>
	{
		public WhsOrderLineWorkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Order);
		}
	}
}
