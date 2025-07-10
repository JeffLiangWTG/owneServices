using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Pick another Pallet ID to Transfer")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse PickAnotherPalletIDToTransfer(Guid transferPK, string oldPalletID, string newPalletID)
		{
			return HandleWebServiceRequest((WhsDocketWebServiceResponse r) => GetWhsTransferCore(r, transferPK, oldPalletID, newPalletID));
		}

		WhsDocketWebServiceResponse GetWhsTransferCore(WhsDocketWebServiceResponse response, Guid transferPK, string oldPalletID, string newPalletID)
		{
			if (string.IsNullOrEmpty(oldPalletID) || string.IsNullOrEmpty(newPalletID))
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("9b289d9c-0bc7-4d4b-b14c-a293c6db4657", "No Pallet ID provided for allocation."));
			}
			else
			{
				var transfer = Factory.Load<WhsTransfer>(transferPK);
				if (transfer != null)
				{
					var linesToTransfer = transfer.Lines.Where(l => l.WE_TransferFromPalletId.EqualsIgnoringCase(oldPalletID)).Cast<WhsTransferLine>().ToArray();
					if (linesToTransfer.Length > 0)
					{
						if (linesToTransfer.Any(l => l.IsPicked))
						{
							response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("86de417d-1acc-464e-9274-8f04f9e1649d", "Current Pallet is already Picked."));
						}
						else
						{
							SwapPalletID(response, newPalletID.ToUpperInvariant(), transfer, linesToTransfer);
						}
					}
					else
					{
						response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("eead6ae8-bc18-4cf6-b616-6f90a19633a6", "Could not find any Transfer Lines."));
					}
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("63e964c4-91b1-49e7-b9ba-08b3244e874b", "Could not find Transfer."));
				}
			}

			return response;
		}

		void SwapPalletID(WhsDocketWebServiceResponse response, string newPalletID, WhsTransfer transfer, WhsTransferLine[] linesToTransfer)
		{
			var palletIDInventory = GetInventoryForPallet(newPalletID);
			if (palletIDInventory.Length > 0)
			{
				SwapPalletID(response, newPalletID, transfer.Client, linesToTransfer, palletIDInventory);

				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					transfer.RunPreSaveValidation();

					if (transfer.HasErrors)
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = transfer.GetErrors().ToUniqueMessageListString();
					}
					else
					{
						Factory.Save(); // To save new commitments to DB.

						response.Docket = new WhsDocketInfo(transfer, shouldCreateDocketLines: false)
						{
							Lines = new WhsDocketLineInfoCollection(transfer.Lines.Cast<WhsTransferLine>()),
							PalletsToTransferCompletely = WhsTransferHelper.GetCompletePalletsForTransfer(Factory, transfer),
						};
					}
				}
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("ec243278-0c6d-4375-b042-a2ab25e94a6c", "Pallet ID '{0}' does not exist or is not Available to Transfer.", newPalletID));
			}
		}

		WhsDocketLine[] GetInventoryForPallet(string newPalletID)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketLineSchema.WE_CurrentInventoryStatus, new[] { InventoryStatus.Codes.Available, InventoryStatus.Codes.Held });
			query.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Finalised);
			query.AddToFilter(WhsDocketLineSchema.WE_PalletID, newPalletID);
			query.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, "");
			query.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0);

			return Factory.Load<WhsDocketLine>(query);
		}

		void SwapPalletID(WhsDocketWebServiceResponse response, string newPalletID, OrgHeader client, WhsTransferLine[] linesToTransfer, WhsDocketLine[] palletIDInventory)
		{
			var qtyToTransfer = linesToTransfer.Sum(l => l.QtyToMoveIncludingMatchingLines);
			var lineToTransfer = linesToTransfer[0];
			if (lineToTransfer.WE_PalletID.EqualsIgnoringCase(lineToTransfer.WE_TransferFromPalletId) && palletIDInventory.Sum(i => i.WE_StockOnHand) != qtyToTransfer)
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("cee2ddad-8f51-4d91-9a4c-369374f874f8", "Unable to transfer Pallet '{0}' as it would result in a partial Pallet Transfer when full Pallet Transfer is required.", newPalletID));
			}
			else
			{
				CheckInventoryAndSwapPalletID(response, newPalletID, client, linesToTransfer, palletIDInventory, lineToTransfer, qtyToTransfer);
			}
		}

		void CheckInventoryAndSwapPalletID(WhsDocketWebServiceResponse response, string newPalletID, OrgHeader client, WhsTransferLine[] linesToTransfer, WhsDocketLine[] palletIDInventory, WhsTransferLine lineToTransfer, decimal qtyToTransfer)
		{
			if (IsAnyTransferLineDifferent(linesToTransfer))
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("9c7d4a29-9046-4201-88ec-f8a3066b27bb", "Pallet ID Neutral is not supported for non-uniform Pallets."));
			}
			else
			{
				var matchingInventory = GetMatchingInventoryOnPallet(palletIDInventory, lineToTransfer);
				if (matchingInventory.Length > 0)
				{
					var isSerialNumberUsed = lineToTransfer.Product.IsSerialNumberUsedAndNotReleaseCaptured(client);

					var errorMessage = SwapPalletID(linesToTransfer, matchingInventory, newPalletID, isSerialNumberUsed, qtyToTransfer);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						response.LogError(ErrorTypes.BusinessValidationError, errorMessage);
					}
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("f5fe8adf-ad8a-459f-9e5c-4668fdb5f6bb", "Unable to transfer Pallet '{0}' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", newPalletID));
				}
			}
		}

		static bool IsAnyTransferLineDifferent(WhsTransferLine[] linesToTransfer)
		{
			var lineToTransfer = linesToTransfer[0];
			return linesToTransfer.Skip(1).Any(l =>
				!IsMatchingInventory(l, lineToTransfer) ||
				l.WE_WL != lineToTransfer.WE_WL ||
				l.WE_PalletID != lineToTransfer.WE_PalletID);
		}

		static WhsDocketLine[] GetMatchingInventoryOnPallet(WhsDocketLine[] palletIDInventory, WhsTransferLine lineToTransfer)
		{
			return palletIDInventory.Where(i =>
				i.WE_WL == lineToTransfer.WE_WL_TransferFrom &&
				IsMatchingInventory(i, lineToTransfer)).ToArray();
		}

		static bool IsMatchingInventory(WhsDocketLine x, WhsDocketLine y)
		{
			return
				x.WE_CurrentInventoryStatus == y.WE_CurrentInventoryStatus &&
				x.WE_OP == y.WE_OP &&
				x.WE_ExpiryDate == y.WE_ExpiryDate &&
				x.WE_PackingDate == y.WE_PackingDate &&
				x.WE_PartAttrib1.EqualsIgnoringCase(y.WE_PartAttrib1) &&
				x.WE_PartAttrib2.EqualsIgnoringCase(y.WE_PartAttrib2) &&
				x.WE_PartAttrib3.EqualsIgnoringCase(y.WE_PartAttrib3) &&
				x.WE_WHC_NKCurrentInventoryHeldCode.EqualsIgnoringCase(y.WE_WHC_NKCurrentInventoryHeldCode);
		}

		string SwapPalletID(WhsTransferLine[] linesToTransfer, WhsDocketLine[] matchingInventory, string newPalletID, bool isSerialNumberUsed, decimal qtyToTransfer)
		{
			string errorMessage;

			var totalStock = matchingInventory.Sum(i => i.WE_StockOnHand);
			if (totalStock >= qtyToTransfer)
			{
				errorMessage = SwapPalletIDCore(linesToTransfer, matchingInventory, newPalletID, isSerialNumberUsed, qtyToTransfer);
			}
			else
			{
				errorMessage = Res.GetString("0c0ccf11-aa0a-4ffb-ac67-1a4dd85a5af4", "Unable to transfer Pallet '{0}' as it does not have enough stock.", newPalletID);
			}

			return errorMessage;
		}

		string SwapPalletIDCore(WhsTransferLine[] linesToTransfer, WhsDocketLine[] matchingInventory, string newPalletID, bool isSerialNumberUsed, decimal qtyToTransfer)
		{
			string errorMessage;

			var pickLinesThatCannotBeChanged = GetPickLinesThatCannotBeModified(matchingInventory);
			var totalStock = matchingInventory.Sum(i => i.WE_StockOnHand);
			var availableQty = totalStock - pickLinesThatCannotBeChanged.Sum(pl => pl.WZ_Units);
			if (availableQty >= qtyToTransfer)
			{
				var committedPickLinesThatCanBeChanged = GetPickLinesThatCanBeChanged(matchingInventory, pickLinesThatCannotBeChanged);
				var serials = SwapPickLinesAndReturnSerialInfo(committedPickLinesThatCanBeChanged);

				var lineToTransfer = linesToTransfer[0];
				var isFullPalletTransfer = lineToTransfer.WE_PalletID.EqualsIgnoringCase(lineToTransfer.WE_TransferFromPalletId);

				foreach (var line in linesToTransfer)
				{
					if (isSerialNumberUsed)
					{
						line.WE_SerialNumber = serials.Dequeue();
					}

					line.WE_TransferFromPalletId = newPalletID;

					if (isFullPalletTransfer)
					{
						line.WE_PalletID = newPalletID;
					}
				}

				errorMessage = "";
			}
			else
			{
				errorMessage = Res.GetString("a37ba599-e48a-409a-9432-c9fbe1670c38", "Unable to transfer Pallet '{0}' as another Job has specifically allocated this Pallet.", newPalletID);
			}

			return errorMessage;

			Queue<ZString> SwapPickLinesAndReturnSerialInfo(WhsPickLine[] committedPickLinesThatCanBeChanged)
			{
				var serialInfos = new Queue<ZString>();

				var nonCommittedQuantity = availableQty - committedPickLinesThatCanBeChanged.Sum(pl => pl.WZ_Units);
				if (committedPickLinesThatCanBeChanged.Length > 0 && nonCommittedQuantity < qtyToTransfer)
				{
					var qtyRequired = qtyToTransfer - nonCommittedQuantity;
					var swappedInventory = SwapPickLines(committedPickLinesThatCanBeChanged, linesToTransfer, isSerialNumberUsed, qtyRequired);
					foreach (var inventory in swappedInventory)
					{
						AddSerialNumbersToCommit(inventory);
					}
				}

				if (isSerialNumberUsed)
				{
					var inventoryToIgnore = new HashSet<ZGuid>(committedPickLinesThatCanBeChanged.Concat(pickLinesThatCannotBeChanged).Select(pl => pl.WZ_WE_InventoryLine));
					var inventoryToUse = new Queue<WhsDocketLine>(matchingInventory.Where(i => !inventoryToIgnore.Contains(i.PK)));

					while (serialInfos.Count < (int)qtyToTransfer)
					{
						AddSerialNumbersToCommit(inventoryToUse.Dequeue());
					}
				}

				return serialInfos;

				void AddSerialNumbersToCommit(WhsDocketLine inventoryLine) => serialInfos.Enqueue(inventoryLine.WE_SerialNumber);
			}
		}

		WhsPickLine[] GetPickLinesThatCannotBeModified(WhsDocketLine[] matchingInventory)
		{
			var pickLineQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, matchingInventory.Select(i => i.PK));
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			var isUnavailableQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			isUnavailableQuery.AddToFilter(WhsPickLineSchema.WZ_IsPicking, true);

			var docketLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, new[] { DocketType.Codes.Adjustment, DocketType.Codes.Transfer });

			isUnavailableQuery.AddSubQuery(docketLineQuery, JoinCondition.Or);
			pickLineQuery.AddToFilter(isUnavailableQuery);

			return Factory.Load<WhsPickLine>(pickLineQuery);
		}

		WhsPickLine[] GetPickLinesThatCanBeChanged(IEnumerable<WhsDocketLine> matchingInventory, WhsPickLine[] pickLinesThatCannotBeChanged)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, matchingInventory.Select(i => i.PK));
			query.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			if (pickLinesThatCannotBeChanged.Length > 0)
			{
				query.AddToFilter(WhsPickLineSchema.PK, SQLComparisonOperator.NotEqual, pickLinesThatCannotBeChanged.Select(pl => pl.PK));
			}

			// this will be used by a stack, so this will basically end up being descending
			query.OrderBy = WhsPickLineSchema.Constants.WZ_Units + OrderByClause.Ascending;

			return Factory.Load<WhsPickLine>(query);
		}

		static IEnumerable<WhsDocketLine> SwapPickLines(IEnumerable<WhsPickLine> committedPickLinesThatCanBeChanged, IEnumerable<WhsTransferLine> linesToTransfer, bool isSerialNumberUsed, decimal qtyRequired)
		{
			var swappedInventory = new List<WhsDocketLine>();
			var pickLinesAvailableToMove = new Stack<WhsPickLine>(committedPickLinesThatCanBeChanged);
			var committedQuantityByInventory = GetTransferCommittedQuantitiesByInventory(linesToTransfer);
			var qtyToSwap = qtyRequired;

			foreach (var pickLineToSwap in committedQuantityByInventory.OrderByDescending(o => o.Value))
			{
				var qtyThatCanSwap = Math.Min(pickLineToSwap.Value, qtyToSwap);

				while (qtyToSwap > 0 && qtyThatCanSwap > 0)
				{
					var pickLine = pickLinesAvailableToMove.Pop();
					if (qtyThatCanSwap >= pickLine.WZ_Units)
					{
						if (isSerialNumberUsed)
						{
							swappedInventory.Add(pickLine.InventoryLine);
						}

						pickLine.WZ_WE_InventoryLine = pickLineToSwap.Key;
						qtyToSwap -= pickLine.WZ_Units;
						qtyThatCanSwap -= pickLine.WZ_Units;
					}
					else
					{
						var clone = pickLine.Split(qtyThatCanSwap);
						clone.WZ_WE_InventoryLine = pickLineToSwap.Key;

						qtyToSwap -= qtyThatCanSwap;
						qtyThatCanSwap = 0m;

						pickLinesAvailableToMove.Push(pickLine);
					}
				}

				if (qtyToSwap == 0)
				{
					break;
				}
			}

			return swappedInventory;
		}

		static Dictionary<ZGuid, ZDecimal> GetTransferCommittedQuantitiesByInventory(IEnumerable<WhsTransferLine> linesToTransfer)
		{
			var committedQuantityByInventory = new Dictionary<ZGuid, ZDecimal>();

			var pickLines = linesToTransfer.SelectMany(l => l.PickLines.Concat(l.MatchingLines.SelectMany(m => m.PickLines)));
			foreach (var pickLine in pickLines)
			{
				if (committedQuantityByInventory.TryGetValue(pickLine.WZ_WE_InventoryLine, out var qty))
				{
					committedQuantityByInventory[pickLine.WZ_WE_InventoryLine] = qty + pickLine.WZ_Units;
				}
				else
				{
					committedQuantityByInventory.Add(pickLine.WZ_WE_InventoryLine, pickLine.WZ_Units);
				}
			}

			return committedQuantityByInventory;
		}
	}
}
