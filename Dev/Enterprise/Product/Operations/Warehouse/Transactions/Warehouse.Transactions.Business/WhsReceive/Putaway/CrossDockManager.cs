using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class CrossDockManager : ICrossDockManager
	{
		#region AllocateCrossDockedLines

		public void AllocateCrossDockedLines(BusinessObjectFactory factory, IEnumerable<WhsReceiveLine> lines)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(lines, nameof(lines));

			var linesArray = lines.Select(rl => rl.PutawayTransferLine ?? (WhsDocketLine)rl).ToArray();
			var hasPickLinesMap = AddFetchHintsAndReturnWhetherAnyPickLines(factory, linesArray);

			for (int i = 0; i < linesArray.Length; i++)
			{
				var line = linesArray[i];

				while (hasPickLinesMap[line.PK] && line.ReservedPickLines.Count > 0)
				{
					var pickableDocketLine = (WhsPickableDocketLine)line.ReservedPickLines[0].DocketLine;
					var crossDockLocation = pickableDocketLine?.PickableDocket.CrossDockLocation;
					if (crossDockLocation == null)
					{
						line.AddRowWarning(Res.GetString("8eee58d2-548f-47c4-b0ed-2665ad8ef613", "Order Cross Dock Location has not been assigned for allocated inventory lines"));
						break;
					}

					var links4NewLine = new LinksWithSameCrossDockLocation(line.ReservedPickLines, crossDockLocation);
					if (links4NewLine.TotalQty < line.WE_TransactionQuantity)
					{
						if (!line.WE_PalletID.IsEmpty)
						{
							throw new FactLoadingException(Res.GetString("28adbe8c-dc6e-434a-9901-802357417ba9", "Cannot Cross Dock a Single Pallet to multiple Cross Dock Locations."));
						}

						// Pallet ID can only be Empty for Receive Lines, since Putaway Transfers must have a Pallet ID.
						SplitLineAndAllocateNewToCrossDockLocation((WhsReceiveLine)line, links4NewLine);
					}
					else
					{
						AllocateToCrossDockLocation(line, links4NewLine.CrossDockLocation);
						break;
					}
				}
			}
		}

		static Dictionary<ZGuid, bool> AddFetchHintsAndReturnWhetherAnyPickLines(BusinessObjectFactory factory, WhsDocketLine[] linesArray)
		{
			foreach (var inventory in linesArray)
			{
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.PK);
			}

			var hasPickLinesMap = new Dictionary<ZGuid, bool>();
			var transactionLinePks = new HashSet<ZGuid>();
			foreach (var inventory in linesArray)
			{
				var reservedPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.PK));
				foreach (var reservedPickLine in reservedPickLines)
				{
					var transactionLinePk = reservedPickLine.WZ_WE_TransactionLine;
					factory.AddFetchHint(WhsDocketLineSchema.PK, transactionLinePk);
					transactionLinePks.Add(transactionLinePk);
				}

				hasPickLinesMap[inventory.PK] = reservedPickLines.Length > 0;
			}

			foreach (var docketLinePk in transactionLinePks)
			{
				var docketLine = factory.Load<WhsDocketLine>(docketLinePk);
				if (docketLine != null)
				{
					factory.AddFetchHint(WhsDocketSchema.PK, docketLine.WE_WD);
				}
			}

			return hasPickLinesMap;
		}

		#endregion

		#region Implementation

		void SplitLineAndAllocateNewToCrossDockLocation(WhsReceiveLine line, LinksWithSameCrossDockLocation links4NewLine)
		{
			var newLine = line.Split(links4NewLine.TotalQty);

			foreach (var pickLine in links4NewLine)
			{
				// We need to delete the old reserved pick line and create a new one to allow the save to occur.
				// Without this, when attempting to reduce the Total Units of the split Inventory, the
				// Over Commit Trigger will reject the update because the existing Reserved Pick Lines have not been
				// allocated to the new Inventory yet.
				var clonedPickLine = (WhsPickLine)pickLine.Clone();

				// the Original Reserved Qty is excluded from the cloning process so we must manually set it.
				((IBusinessObjectInternals)clonedPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = (decimal)pickLine.WZ_OriginalReservedQty;
				clonedPickLine.IsReserveLine = true;
				clonedPickLine.WZ_WE_InventoryLine = newLine.PK;
				pickLine.Delete();
			}

			AllocateToCrossDockLocation(newLine, links4NewLine.CrossDockLocation);
		}

		void AllocateToCrossDockLocation(WhsDocketLine line, WhsLocation crossDockLocation)
		{
			line.WE_WL = crossDockLocation.PK;
		}

		class LinksWithSameCrossDockLocation : List<WhsPickLine>
		{
			#region ctor

			public LinksWithSameCrossDockLocation(WhsPickLineCollection sourceCollection, WhsLocation crossDockLocation)
				: base(sourceCollection.Count)
			{
				CrossDockLocation = crossDockLocation;
				TotalQty = 0;

				foreach (var pickLine in sourceCollection)
				{
					var pickableDocketLine = (WhsPickableDocketLine)pickLine.DocketLine;
					if (pickableDocketLine.PickableDocket.WD_WL_CrossDock == crossDockLocation.PK)
					{
						TotalQty += pickLine.ReservedQuantity;
						Add(pickLine);
					}
				}
			}

			#endregion

			#region Location

			public WhsLocation CrossDockLocation
			{
				get;
				private set;
			}

			#endregion

			#region TotalQty

			public ZDecimal TotalQty
			{
				get;
				private set;
			}

			#endregion
		}

		#endregion
	}
}
