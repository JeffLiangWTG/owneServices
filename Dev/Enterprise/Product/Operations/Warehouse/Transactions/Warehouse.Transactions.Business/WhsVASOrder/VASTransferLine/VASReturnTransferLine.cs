using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class VASReturnTransferLine : ILineToPutaway
	{
		public VASReturnTransferLine(WhsTransferLine line)
		{
			TransferLine = Argument.NotNull(line, nameof(line));

			if (!line.IsMainTransactionLine())
			{
				throw new InvalidOperationException("Should only wrap Main Transfer Lines as VASTransferLines.");
			}
		}

		public WhsTransferLine TransferLine { get; }

		#region GetLinesToPutawayFromTransfer

		public static IEnumerable<VASReturnTransferLine> GetLinesToPutawayFromTransfer(WhsTransfer returnTransfer)
		{
			IEnumerable<VASReturnTransferLine> result;

			if (returnTransfer == null)
			{
				result = Enumerable.Empty<VASReturnTransferLine>();
			}
			else
			{
				EnsureValidTransfer(returnTransfer);
				result = returnTransfer.Lines.Cast<WhsTransferLine>().Select(GetPutawayLineFromTransferLine);
			}

			return result;
		}

		// cache the wrapped transferline to get the same instance for the same transferline making it behave like a BizO
		public static VASReturnTransferLine GetPutawayLineFromTransferLine(WhsTransferLine transferLine)
		{
			var key = "PutawayManagerForVASTransferLine|GetPutawayLineFromTransferLine|" + transferLine.PK;
			return transferLine.Factory.GetCachedValue(key, () => new VASReturnTransferLine(transferLine));
		}

		static void EnsureValidTransfer(WhsTransfer returnTransfer)
		{
			var query = new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, returnTransfer.PK);

			if (returnTransfer.Factory.LoadTop1<WhsVASOrder>(query) != null)
			{
				throw new InvalidOperationException("Should not do Putaway for the Into Service Area VAS Order Transfer.");
			}
		}

		#endregion

		BusinessObjectFactory ILineToPutawayInfo.Factory => TransferLine.Factory;

		ZGuid ILineToPutawayInfo.PK => TransferLine.PK;

		ZGuid ILineToPutawayInfo.DocketPK => TransferLine.WE_WD;

		ZGuid ILineToPutawayInfo.ProductPK => TransferLine.WE_OP;

		ZGuid ILineToPutawayInfo.WarehousePK => TransferLine.WarehousePK;

		WhsLocation ILineToPutaway.Location => TransferLine.Location;

		OrgSupplierPart ILineToPutaway.Product => TransferLine.SupplierPart;

		ZString ILineToPutawayInfo.InventoryStatus => InventoryStatus.Codes.Available; // we will make the stock available after we transfer it

		ZString ILineToPutawayInfo.InventoryHeldCode => string.Empty;

		ZString ILineToPutawayInfo.PalletID
		{
			get => TransferLine.WE_PalletID;
			set => TransferLine.WE_PalletID = value;
		}

		ZDecimal ILineToPutawayInfo.QuantityToPutaway => TransferLine.QtyToMoveIncludingMatchingLines;
		ZDecimal ILineToPutawayInfo.PackQuantity => TransferLine.PackQtyIncludingMatchingLines;

		ZGuid ILineToPutawayInfo.LocationPK
		{
			get { return TransferLine.WE_WL; }
			set { TransferLine.WE_WL = value; }
		}

		ZString ILineToPutawayInfo.PackType
		{
			get { return TransferLine.WE_F3_NKPackType; }
			set { TransferLine.WE_F3_NKPackType = value; }
		}

		bool ILineToPutawayInfo.CheckPalletIDExists => false;

		// can only put away fully committed stock, this should always be true for return transfers created from VAS Orders
		bool ILineToPutawayInfo.IsValidToPutaway => TransferLine.QtyCommittedIncludingMatchingLines == TransferLine.QtyToMoveIncludingMatchingLines;

		ILineToPutaway ILineToPutaway.Split(ZDecimal quantityForNewLine)
		{
			if (quantityForNewLine <= 0m || quantityForNewLine >= TransferLine.QtyToMoveIncludingMatchingLines)
			{
				throw new ArgumentException("Quantity for splitting Transfer line should be greater than Zero and less than the Total Units.");
			}

			var currentQtyToSplit = quantityForNewLine;
			var splitTransferLine = TransferLine;

			foreach (WhsTransferLine matchingLine in TransferLine.MatchingLines.OrderByDescending(l => l.WE_TransactionQuantity))
			{
				if (currentQtyToSplit >= matchingLine.WE_TransactionQuantity)
				{
					SplitOffLineFully(splitTransferLine, matchingLine);
					splitTransferLine = splitTransferLine == TransferLine ? matchingLine : splitTransferLine;
					currentQtyToSplit -= matchingLine.WE_TransactionQuantity;
				}
				else
				{
					var newLine = SplitOffLinePartially(currentQtyToSplit, splitTransferLine, matchingLine);
					splitTransferLine = splitTransferLine == TransferLine ? newLine : splitTransferLine;
					currentQtyToSplit = 0m;
				}

				if (currentQtyToSplit == 0m)
				{
					break;
				}
			}

			if (currentQtyToSplit > 0m)
			{
				var newLine = SplitOffLinePartially(currentQtyToSplit, splitTransferLine, TransferLine);
				splitTransferLine = splitTransferLine == TransferLine ? newLine : splitTransferLine;
			}

			return GetPutawayLineFromTransferLine(splitTransferLine);
		}

		WhsTransferLine SplitOffLinePartially(ZDecimal currentQtyToSplit, WhsTransferLine splitTransferLine, WhsTransferLine lineToClone)
		{
			// move quantity across to cloned line
			var clone = lineToClone.Clone<WhsTransferLine>();
			clone.WE_WD = lineToClone.WE_WD;
			clone.WE_TransactionQuantity = currentQtyToSplit;
			lineToClone.WE_TransactionQuantity -= currentQtyToSplit;

			// move committed quantity across to cloned line
			var pickLine = lineToClone.PickLines.Single();

			var newPickLine = clone.PickLines.AddNew();
			using (newPickLine.GetValidationSuspender())
			{
				newPickLine.WZ_Units = currentQtyToSplit;
				newPickLine.WZ_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
			}

			using (pickLine.GetValidationSuspender())
			{
				pickLine.WZ_Units -= currentQtyToSplit;
			}

			SplitOffLineFully(splitTransferLine, clone);

			return clone;
		}

		void SplitOffLineFully(WhsTransferLine splitTransferLine, WhsTransferLine lineToSplit)
		{
			if (splitTransferLine == TransferLine)
			{
				lineToSplit.WE_WE_MatchingLine = ZGuid.Empty; // make the line a top level transfer line
			}
			else
			{
				splitTransferLine.MatchingLines.Add(lineToSplit);
			}
		}
	}
}
