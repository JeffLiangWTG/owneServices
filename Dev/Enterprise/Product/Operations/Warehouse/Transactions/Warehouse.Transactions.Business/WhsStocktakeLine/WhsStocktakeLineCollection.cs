using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeLineCollection : ActiveBusinessObjectCollection<WhsStocktakeLine>
	{
		public WhsStocktakeLineCollection(WhsStocktake master)
			: base(master)
		{
		}

		#region GetSortComparerForProperty

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == WhsStocktakeLine.Schema.LocationString)
			{
				return new LocationComparer<WhsStocktakeLine>(property, direction, stocktakeLine => stocktakeLine.Location);
			}
			return base.GetSortComparerForProperty(property, direction);
		}

		#endregion

		#region Related Entities

		#region Master

		public WhsStocktake Master
		{
			get { return (WhsStocktake)Relationship.Master; }
		}

		#endregion

		#endregion

		#region SetDefaultsForNewElementCore

		protected override void SetDefaultsForNewElementCore(WhsStocktakeLine bizO)
		{
			base.SetDefaultsForNewElementCore(bizO);

			var stocktakeLine = bizO;
			stocktakeLine.WU_WS = Master.PK;
			stocktakeLine.WU_IsManuallyAdded = true;
			stocktakeLine.WU_LineNo = GetNextLineNo();
			stocktakeLine.WU_OH_Client = Master.WS_OH_Client;
			stocktakeLine.WU_Status = StocktakeLineStatus.Codes.Open;
			stocktakeLine.WU_TotalCounts = Master.CurrentCountColumnNumber;
			stocktakeLine.WU_InventoryStatus = InventoryStatus.Codes.Available;

			if (Master.WS_WL_Location.IsValid)
			{
				stocktakeLine.WU_WL = Master.WS_WL_Location;
			}
		}

#if DEBUG
		public
#endif
		ZInt GetNextLineNo()
		{
			var maximumCount = Master.Lines.Count == 0 ? ZInt.Zero : Master.Lines.Max(l => l.WU_LineNo);
			return maximumCount + 1;
		}

		#endregion

		#region GetDuplicateStocktakeLine

		public WhsStocktakeLine GetDuplicateStocktakeLine(WhsStocktakeLine line)
		{
			if (line.LocationWhsGuid == Master.WS_WW_Whs)
			{
				var openStocklines = this.Where(l => l.WU_Status == StocktakeLineStatus.Codes.Open && l.PK != line.PK);
				return openStocklines.FirstOrDefault(l => l.IsEqualLine(line));
			}
			else
			{
				throw new InvalidOperationException("Line does not belong to this stocktake.");
			}
		}

		#endregion

		#region ValidateDuplicateLine

		public void ValidateDuplicateLine()
		{
			foreach (WhsStocktakeLine line in this.Where(l => l.WU_Status == StocktakeLineStatus.Codes.Open))
			{
				var validation = line.Validation as WhsStocktakeLineValidationForManuallyAddedLines; // Check is manually added line
				if (validation != null)
				{
					validation.ValidateDuplicateLine();
				}
			}
		}

		#endregion
	}

	#region Non Dependant WhsStocktakeLineCollection

	public class WhsStocktakeLineCollectionND : BusinessObjectCollection<WhsStocktakeLine>
	{
		public WhsStocktakeLineCollectionND(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsStocktakeLineCollectionND(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}

	#endregion
}
