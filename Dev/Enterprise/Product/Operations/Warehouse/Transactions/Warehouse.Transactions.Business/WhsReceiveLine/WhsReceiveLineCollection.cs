using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveLineCollection : WhsDocketLineCollection
	{
		public WhsReceiveLineCollection(WhsReceive master)
			: base(master)
		{
		}

		public WhsReceiveLineCollection(WhsReceive master, ZQuery filter)
			: base(master, filter)
		{
		}

		public new WhsReceiveLine this[int index] => (WhsReceiveLine)(base[index]);

		protected override bool AllowNew
			=> !Docket.IsDeleted
			&& !Docket.IsCreatedFromPickByBOM
			&& !ReceiveLockedForTaskPutaway
			&& base.AllowNew;

		bool ReceiveLockedForTaskPutaway => Docket is WhsReceive receive && receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared;

		protected override bool AllowRemoveCore
			=> !ReceiveLockedForTaskPutaway;

		public virtual new WhsReceiveLine AddNew() => (WhsReceiveLine)base.AddNew();

		protected override IEnumerable<ZString> AllowNewAdditionValidDocketStatuses => new ZString[] { DocketStatus.Codes.Putaway };
		protected override IEnumerable<ZString> AllowRemoveAdditionValidDocketStatuses => new ZString[] { DocketStatus.Codes.Putaway };

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return property.Name == WhsReceiveLine.Schema.DestLocation
				? new LocationComparer<WhsReceiveLine>(property, direction, receiveLine => GetDestLocation(receiveLine))
				: base.GetSortComparerForProperty(property, direction);
		}

		WhsLocation GetDestLocation(WhsReceiveLine receiveLine)
		{
			WhsLocation location;
			if (receiveLine.HasPutawayTransfer)
			{
				location = receiveLine.PutawayTransferLine?.Location;
			}
			else
			{
				var currentLocationClass = receiveLine.Location?.LocationType?.WLT_LocationClass ?? ZString.Empty;
				location = currentLocationClass == LocationClasses.Codes.DDL ? null : receiveLine.Location;
			}

			return location;
		}
	}
}
