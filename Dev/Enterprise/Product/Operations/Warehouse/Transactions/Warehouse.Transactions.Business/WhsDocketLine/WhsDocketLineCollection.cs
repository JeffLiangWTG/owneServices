using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketLineCollection : ActiveBusinessObjectCollection<WhsDocketLine>, IBindingList
	{
		protected WhsDocketLineCollection(WhsDocket master)
			: base(master)
		{
		}

		protected WhsDocketLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected WhsDocketLineCollection(WhsDocket master, ZQuery filter)
			: base(master, filter)
		{
		}

		protected WhsDocketLineCollection(WhsDocketLine master, ZQuery filter, SchemaGuidColumn fk)
			: base(master.Factory, master, filter, fk)
		{
		}

		public WhsDocket Docket
		{
			get
			{
				WhsDocket docket;

				var parentLine = ParentLine;
				if (parentLine != null)
				{
					docket = parentLine.IsDeleted ? null : parentLine.Docket;
				}
				else
				{
					docket = (WhsDocket)Relationship.Master;
				}

				return docket;
			}
		}

		protected WhsDocketLine ParentLine => Relationship.Master as WhsDocketLine;

		#region OnAdded

		protected override void OnAdded(WhsDocketLine lineToAdd)
		{
			base.OnAdded(lineToAdd);

			if (lineToAdd.WE_LineNo == 0 && Docket.NeedToAutoGenerateLineNumbersOnCreation)
			{
				lineToAdd.WE_LineNo = GetNextLineNo();
			}
		}

		ZShort GetNextLineNo()
		{
			ZShort maxNo = 0;
			foreach (var line in this)
			{
				var currentLineNo = line.WE_LineNo;
				if (maxNo < currentLineNo)
				{
					maxNo = currentLineNo;
				}
			}
			return maxNo != short.MaxValue ? maxNo + 1 : ZShort.Zero;
		}

		#endregion

		#region IBindingList Members

		#region AllowRemove

		bool IBindingList.AllowRemove => (Docket?.IsDeleted ?? true) || (AllowRemoveValidDocketStatuses.Contains(Docket.WD_DocketStatus) && AllowRemoveCore);

		protected virtual bool AllowRemoveCore => true;

		List<ZString> AllowRemoveValidDocketStatuses
		{
			get
			{
				var result = new List<ZString>(new ZString[]
				{
					DocketStatus.Codes.New,
					DocketStatus.Codes.Entered,
					DocketStatus.Codes.Held
				});

				result.AddRange(AllowRemoveAdditionValidDocketStatuses);

				return result;
			}
		}

		protected virtual IEnumerable<ZString> AllowRemoveAdditionValidDocketStatuses => Array.Empty<ZString>();

		#endregion

		#region AllowNew

		protected override bool AllowNew
		{
			get
			{
				var docket = Docket;
				return docket.WD_OH_Client.IsValid &&
					docket.WD_WW_Whs.IsValid &&
					AllowNewValidDocketStatuses.Contains(docket.WD_DocketStatus);
			}
		}

		List<ZString> AllowNewValidDocketStatuses
		{
			get
			{
				var result = new List<ZString>(new ZString[]
				{
					DocketStatus.Codes.New,
					DocketStatus.Codes.Entered,
					DocketStatus.Codes.Held
				});

				result.AddRange(AllowNewAdditionValidDocketStatuses);

				return result;
			}
		}

		protected virtual IEnumerable<ZString> AllowNewAdditionValidDocketStatuses => Array.Empty<ZString>();

		#endregion

		#endregion

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case nameof(WhsDocketLine.WE_WL):
				case nameof(WhsDocketLine.LocationString):
					return new LocationComparer<WhsDocketLine>(property, direction, docketLine => docketLine.Location);
				case nameof(WhsDocketLine.WE_WL_TransferFrom):
				case nameof(WhsTransferLine.TransferFromLocationString):
					return new LocationComparer<WhsDocketLine>(property, direction, docketLine => ((WhsTransferLine)docketLine).TransferFromLocation);
				default:
					return base.GetSortComparerForProperty(property, direction);
			}
		}
	}

	#region Non Dependant WhsDocketLineCollection

	public class WhsDocketLineCollectionND : BusinessObjectCollection<WhsDocketLine>
	{
		public WhsDocketLineCollectionND(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}

	#endregion
}
