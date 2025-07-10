using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region TryToTransferStock

		[WebMethod(Description = "If all required data entered then Transfer and Finalise all matching Transfer lines, otherwise validate available data.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public TransferPutawayWebServiceResponse TryToTransferStock(Guid transferPK, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			return HandleWebServiceRequest<TransferPutawayWebServiceResponse>(r => TryToTransferStockCore(r, transferPK, transferLineInfo, isAttributesSet, transferTaskPK));
		}

		void TryToTransferStockCore(TransferPutawayWebServiceResponse response, Guid transferPK, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			ValidateDocketLineInfo(response, transferLineInfo);
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var transfer = Factory.Load<WhsTransfer>(transferPK);
				if (transfer != null)
				{
					if (transfer.CheckTransferIsMasterTransfer(response))
					{
						TransferStock(response, transfer, transferLineInfo, isAttributesSet, transferTaskPK);
					}
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("6D11A5FC-F1BF-422E-8F77-EA9CF7A2D361", "Transfer with PK = '{0}' cannot be found.", transferPK));
				}
			}
		}

		void TransferStock(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			var transferLine = Factory.Load<WhsTransferLine>(transferLineInfo.PK);
			var isStrictLinePutaway = transferLine != null;

			WhsTransferLine[] matchingTransferLines;

			if (isStrictLinePutaway)
			{
				var specifiedLine = GetSpecifiedTransferLine(response, transfer, transferLine, transferLineInfo, transferTaskPK);
				matchingTransferLines = specifiedLine != null ? [specifiedLine] : Array.Empty<WhsTransferLine>();
			}
			else
			{
				matchingTransferLines = GetAllMatchingTransferLines(response, transfer, transferLineInfo, isAttributesSet, transferTaskPK).ToArray();
			}

			if (response.NoError())
			{
				var factory = transfer.Factory;
				var connection = ((IDbConnected)factory).Connection;
				using (var manager = connection.BeginTransactionWithManager())
				{
					transfer.RunPreSaveValidation();
					InvalidateTransferDataForTest(transfer);

					if (transfer.HasErrors)
					{
						if (string.IsNullOrEmpty(response.ErrorMessage))
						{
							response.LogBusinessValidationError(Res.GetString("43a7128e-3364-4b8c-99b1-a9914a9f7f76", "Unknown errors occur when transferring stock, Please open transfer {0} in the Desktop application to fix them.", transfer.WD_DocketID));
						}
						manager.RollbackTransaction();
					}
					else
					{
						factory.Save();

						ThrowExceptionForDataIntegrityTestDuringTransferPutaway();

						FinaliseTransferLines(response, transfer, matchingTransferLines, transferLineInfo, isAttributesSet, isStrictLinePutaway, transferTaskPK);
						manager.CommitTransaction();
					}
				}
			}
		}

		partial void ThrowExceptionForDataIntegrityTestDuringTransferPutaway();
		partial void InvalidateTransferDataForTest(WhsTransfer transfer);

		#region GetSpecifiedTransferLine

		WhsTransferLine GetSpecifiedTransferLine(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsTransferLine transferLine, WhsDocketLineInfo transferLineInfo, Guid transferTaskPK)
		{
			WhsTransferLine result = null;

			if (transferTaskPK != Guid.Empty && transferLine.WE_P9_Task != transferTaskPK)
			{
				response.LogBusinessValidationError(TransferLineAssignedToAnotherUser);
			}
			else if (transferLine.IsHeldForTransfer)
			{
				response.IsValidProduct = true;
				response.IsValidInventoryHeldCode = true;
				response.IsValidDestLocation = true;
				response.IsValidDestPalletID = true;
				response.IsValidAttributes = true;
				response.TotalQuantityAvailableForPutaway = transferLine.QtyToMoveIncludingMatchingLines;
				response.TotalQuantityTransferred = transferLineInfo.Qty;

				result = SplitTransferLineIfRequired(transfer, transferLine, transferLineInfo.Qty);
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("0e5a8d03-596c-413a-861e-c5e6a6786ebb", "Transfer Line must be Picked."));
			}

			return result;
		}

		#endregion

		#region GetAllMatchingTransferLines

		IEnumerable<WhsTransferLine> GetAllMatchingTransferLines(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			var matchingTransferLines = Enumerable.Empty<WhsTransferLine>();

			var destLocation = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, transferLineInfo.DestLocation);
			if (response.NoError())
			{
				response.IsValidDestLocation = (destLocation != null);
				if (response.IsValidDestLocation)
				{
					matchingTransferLines = GetAllMatchingTransferLinesCore(response, transfer, transferLineInfo, isAttributesSet, transferTaskPK);
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("ca78085d-3712-4292-8606-55f7a1d9574d", "Location '{0}' cannot be found.", transferLineInfo.DestLocation));
				}
			}

			return matchingTransferLines;
		}

		IEnumerable<WhsTransferLine> GetAllMatchingTransferLinesCore(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			IEnumerable<WhsTransferLine> result;

			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var pickedTransferLinesAssignedToUser = transfer.Lines.Cast<WhsTransferLine>().Where(l => l.IsHeldForTransfer &&
				(l.WE_GS_NKPutawayBy.IsEmpty || l.WE_GS_NKPutawayBy == staff.GS_Code)).ToArray();

			// to make sure we only finalised lines assigned to current logged in user
			if (pickedTransferLinesAssignedToUser.Length == 0)
			{
				result = Enumerable.Empty<WhsTransferLine>();
				response.LogBusinessValidationError(Res.GetString("a27ea2ef-294b-4bf3-b0e3-95652790ded8", "None of the Transfer Lines are Held For Transfer."));
			}
			else if (!string.IsNullOrEmpty(transferLineInfo.PalletID))
			{
				result = GetMatchingTransferLinesForFullPalletPutaway(response, transferLineInfo, transferTaskPK, pickedTransferLinesAssignedToUser);
			}
			else
			{
				var pickedTransferLinesAssignedToUserToConsider = pickedTransferLinesAssignedToUser.Where(line => transferTaskPK == Guid.Empty || line.WE_P9_Task == transferTaskPK).ToArray();
				if (pickedTransferLinesAssignedToUserToConsider.Length == 0)
				{
					result = Enumerable.Empty<WhsTransferLine>();
					response.LogBusinessValidationError(Res.GetString("d875c87c-ae6c-4c3c-8da5-369a9a55b10c", "None of the Transfer Lines are assigned to the user."));
				}
				else if (transferLineInfo.Product != null && transferLineInfo.Product.PK != Guid.Empty)
				{
					result = GetAllMatchingTransferLines_SingleProductPutaway(response, transfer, pickedTransferLinesAssignedToUserToConsider, transferLineInfo, isAttributesSet);
				}
				else
				{
					result = pickedTransferLinesAssignedToUserToConsider;
				}
			}

			return result;
		}

		#region GetAllMatchingTransferLines_FullPalletPutaway

		IEnumerable<WhsTransferLine> GetMatchingTransferLinesForFullPalletPutaway(TransferPutawayWebServiceResponse response, WhsDocketLineInfo transferLineInfo, Guid transferTaskPK, WhsTransferLine[] pickedTransferLinesAssignedToUser)
		{
			var transferLines = Enumerable.Empty<WhsTransferLine>();
			var matchingTransferLinesForFullPalletPutaway = GetAllMatchingTransferLines_FullPalletPutaway(response, transferLineInfo, pickedTransferLinesAssignedToUser);
			if (response.NoError())
			{
				if (transferTaskPK == Guid.Empty || matchingTransferLinesForFullPalletPutaway.All(line => line.WE_P9_Task == transferTaskPK))
				{
					transferLines = matchingTransferLinesForFullPalletPutaway;
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("76aa401a-0970-4b14-9fba-f00b89a797f3", "Some of the transfer lines on the full pallet transfer are not assigned to the user."));
				}
			}

			return transferLines;
		}

		IEnumerable<WhsTransferLine> GetAllMatchingTransferLines_FullPalletPutaway(TransferPutawayWebServiceResponse response, WhsDocketLineInfo transferLineInfo, IEnumerable<WhsTransferLine> transferLines)
		{
			var result = transferLines.Where(l => l.WE_TransferFromPalletId.EqualsIgnoringCase(transferLineInfo.PalletID)).ToArray();

			response.IsValidSourcePalletID = result.Length != 0;
			if (!response.IsValidSourcePalletID)
			{
				response.LogBusinessValidationError(Res.GetString("9a7b09d9-cfbd-4a82-83c7-61defddbbe2b", "Picked Source Pallet ID '{0}' could not be found.", transferLineInfo.PalletID));
			}

			return result;
		}

		#endregion

		#region GetAllMatchingTransferLines_SingleProductPutaway

		IEnumerable<WhsTransferLine> GetAllMatchingTransferLines_SingleProductPutaway(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsTransferLine[] transferLines, WhsDocketLineInfo transferLineInfo, bool isAttributesSet)
		{
			var result = Enumerable.Empty<WhsTransferLine>();
			var matchingTransferLines = transferLines.Where(l => l.WE_OP == transferLineInfo.Product.PK).ToArray();

			response.IsValidProduct = matchingTransferLines.Length > 0;
			if (!response.IsValidProduct)
			{
				response.LogBusinessValidationError(Res.GetString("86888379-e8c0-4fb3-822d-c8daf44954e2", "Picked Product '{0}' could not be found.", transferLineInfo.Product.Code));
			}
			else
			{
				matchingTransferLines = matchingTransferLines.Where(l => l.WE_WHC_NKCurrentInventoryHeldCode.EqualsIgnoringCase(transferLineInfo.InventoryHeldCode)).ToArray();
				response.IsValidInventoryHeldCode = (matchingTransferLines.Any());
				if (!response.IsValidInventoryHeldCode)
				{
					response.LogBusinessValidationError(Res.GetString("e43876bb-cb85-4f70-82c9-b20346b34c66", "No inventory with Hold Code '{0}' await putting away.", transferLineInfo.InventoryHeldCode));
				}
				else if (transferLineInfo.Qty > 0m)
				{
					result = GetAllMatchingTransferLines_SingleProductPutaway_QuantityAndAttributes(response, transfer, matchingTransferLines, transferLineInfo, isAttributesSet);
				}
				else
				{
					result = matchingTransferLines;
				}
			}

			return result;
		}

		IEnumerable<WhsTransferLine> GetAllMatchingTransferLines_SingleProductPutaway_QuantityAndAttributes(TransferPutawayWebServiceResponse response, WhsTransfer transfer, IReadOnlyList<WhsTransferLine> transferLines, WhsDocketLineInfo transferLineInfo, bool isAttributesSet)
		{
			var result = transferLines;

			response.TotalQuantityAvailableForPutaway = transferLines.Sum(l => l.QtyToMoveIncludingMatchingLines);
			if (response.TotalQuantityAvailableForPutaway < transferLineInfo.Qty)
			{
				var part = transferLines[0].SupplierPart;
				var decimalsFormat = string.Format(CultureInfo.CurrentCulture, "0.{0}", new string('0', part.OP_CountDecimalPlaces));
				var qtyString = transferLineInfo.Qty.ToString(decimalsFormat, CultureInfo.CurrentCulture);
				var qtyAvailableString = response.TotalQuantityAvailableForPutaway.ToString(decimalsFormat, CultureInfo.CurrentCulture);

				response.LogBusinessValidationError(Res.GetString("4e4af357-4bc9-49cb-b438-6e1f4ec0f99a", "Cannot putaway {0} {2} only {1} {2} available to put away.", qtyString, qtyAvailableString, transferLineInfo.QtyUQ));
			}
			else if (isAttributesSet)
			{
				var matchingTransferLines = transferLines.Where(l =>
					l.WE_PartAttrib1.EqualsIgnoringCase(transferLineInfo.Attribute1) &&
					l.WE_PartAttrib2.EqualsIgnoringCase(transferLineInfo.Attribute2) &&
					l.WE_PartAttrib3.EqualsIgnoringCase(transferLineInfo.Attribute3) &&
					l.WE_SerialNumber.EqualsIgnoringCase(transferLineInfo.SerialNumber) &&
					l.WE_ExpiryDate.Equals(transferLineInfo.ExpiryDate.Date) &&
					l.WE_PackingDate.Equals(transferLineInfo.PackingDate.Date)).ToArray();

				result = matchingTransferLines;
				response.IsValidAttributes = matchingTransferLines.Length > 0;
				if (!response.IsValidAttributes)
				{
					response.LogBusinessValidationError(Res.GetString("0682728c-383e-4cb0-99c4-11b2c6b53eeb", "None of the stock for putaway match entered attributes."));
				}
				else
				{
					var qtyAvailableToTransfer = matchingTransferLines.Sum(l => l.QtyToMoveIncludingMatchingLines);
					if (qtyAvailableToTransfer > transferLineInfo.Qty)
					{
						var transferLinesToPopulateAndFinalise = new List<WhsTransferLine>();
						var qtySelected = 0m;

						for (var i = 0; i < matchingTransferLines.Length && transferLineInfo.Qty != qtySelected; i++)
						{
							var transferLine = matchingTransferLines[i];
							var qtyToTransfer = Math.Min(transferLineInfo.Qty - qtySelected, transferLine.QtyToMoveIncludingMatchingLines);
							qtySelected += qtyToTransfer;

							var lineTransferred = SplitTransferLineIfRequired(transfer, transferLine, qtyToTransfer);
							transferLinesToPopulateAndFinalise.Add(lineTransferred);
						}

						result = transferLinesToPopulateAndFinalise;
					}
				}
			}

			return result;
		}

		WhsTransferLine SplitTransferLineIfRequired(WhsTransfer transfer, WhsTransferLine transferLine, decimal transferringQty)
		{
			WhsTransferLine result;

			if (transferringQty < transferLine.QtyToMoveIncludingMatchingLines)
			{
				transfer.Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, string.Format(Culture.Current, "RF: Line No. {0} - [Qty changed from {1}]", transferLine.WE_LineNo, transferLine.WE_TransactionQuantity));  // RF Log message.

				var clone = transferLine.Clone<WhsTransferLine>();
				clone.WE_DocketLineStatus = transferLine.WE_DocketLineStatus;
				clone.WE_GS_NKPutawayBy = transferLine.WE_GS_NKPutawayBy;
				clone.WE_WE_MatchingLine = ZGuid.Empty;
				clone.WE_P9_Task = transferLine.WE_P9_Task;
				transfer.Lines.Add(clone);

				var newLines = SplitQuantityAndPickLines(transferLine, clone, transferringQty);
				foreach (var line in newLines)
				{
					transfer.Lines.Add(line);
				}

				if (transferLine.IsPicked)
				{
					// Recreate In-Transit inventory on the two lines, handles matching lines, child creation etc.
					transferLine.CreateInTransitInventory();
					clone.CreateInTransitInventory();
				}

				result = clone;
			}
			else
			{
				result = transferLine;
			}

			return result;
		}

		static IEnumerable<WhsTransferLine> SplitQuantityAndPickLines(WhsTransferLine transferLineToSplitFrom, WhsTransferLine cloneToSplitTo, decimal qtyToSplit)
		{
			// go through the matching lines on the main transfer line first to transfer PickLines across
			var matchingLinesOrdered = transferLineToSplitFrom.MatchingLines.OrderBy(l => l.WE_TransactionQuantity).ToArray();
			var newLines = new List<WhsTransferLine>();
			foreach (WhsTransferLine lineToConsider in matchingLinesOrdered)
			{
				var qtyPicked = lineToConsider.GetQtyCommittedToThisLine();
				if (qtyPicked <= qtyToSplit) // can we transfer the picked qty entirely
				{
					MovePickLine(cloneToSplitTo, lineToConsider, qtyPicked);
					qtyToSplit -= qtyPicked;
				}
				else
				{
					SplitPickLineAndCreateNewTransferLineAsNeeded(lineToConsider);
				}

				if (qtyToSplit == 0m)
				{
					break;
				}
			}

			// if we still need to split some picked qty, split pick line from the original transfer line
			if (qtyToSplit > 0m && transferLineToSplitFrom.PickLines.Any())
			{
				SplitPickLineAndCreateNewTransferLineAsNeeded(transferLineToSplitFrom);
			}

			// no pick lines to transfer, simply split the quantity of the two lines
			if (qtyToSplit > 0m)
			{
				cloneToSplitTo.WE_TransactionQuantity = qtyToSplit;
				transferLineToSplitFrom.WE_TransactionQuantity -= qtyToSplit;
			}

			return newLines;

			void SplitPickLineAndCreateNewTransferLineAsNeeded(WhsTransferLine lineToSplitFrom)
			{
				var newLine = SplitPickLine(cloneToSplitTo, qtyToSplit, lineToSplitFrom);
				qtyToSplit = 0m;

				if (newLine != null)
				{
					newLines.Add(newLine);
				}
			}
		}

		static void MovePickLine(WhsTransferLine transferLineToTransferPickLineTo, WhsTransferLine matchingLineWithPickLine, ZDecimal qtyPicked)
		{
			// if transfer line has no PickLines just directly assign the PickLine and delete the matching line
			if (!transferLineToTransferPickLineTo.PickLines.Any())
			{
				var pickLine = matchingLineWithPickLine.PickLines.Single();
				pickLine.WZ_WE_TransactionLine = transferLineToTransferPickLineTo.PK;
				transferLineToTransferPickLineTo.WE_TransactionQuantity = qtyPicked;
				matchingLineWithPickLine.Delete();
			}
			// otherwise just move the matching line to the new line
			else
			{
				matchingLineWithPickLine.WE_WE_MatchingLine = transferLineToTransferPickLineTo.PK;
			}
		}

		static WhsTransferLine SplitPickLine(WhsTransferLine transferLineToTransferPickLineTo, decimal qtyToSplit, WhsTransferLine transferLineWithPickLine)
		{
			var pickLine = transferLineWithPickLine.PickLines.Single();
			var newPickLine = (WhsPickLine)pickLine.Clone(new BusinessObjectCloneArgs(new[] { WhsPickLineSchema.Constants.WZ_Units, WhsPickLineSchema.Constants.WZ_WE_TransactionLine })); // no need to clone the units or transaction line.
			WhsPickLine.TransferQtyAcrossPickLines(newPickLine, pickLine, qtyToSplit);

			WhsTransferLine newMatchingLine = null;

			pickLine.WZ_IsPicking = !pickLine.IsPicked;
			transferLineWithPickLine.WE_TransactionQuantity -= qtyToSplit; // reduce transferline by amount we are splitting

			// if transfer line has no PickLines just directly assign the PickLine
			if (!transferLineToTransferPickLineTo.PickLines.Any())
			{
				newPickLine.WZ_WE_TransactionLine = transferLineToTransferPickLineTo.PK;
				transferLineToTransferPickLineTo.WE_TransactionQuantity = qtyToSplit;
			}
			// otherwise add a new Matching Line to take the split PickLine
			else
			{
				newMatchingLine = transferLineToTransferPickLineTo.CreateMatchingLineWithQuantity(qtyToSplit);
				newMatchingLine.WE_DocketLineStatus = transferLineToTransferPickLineTo.WE_DocketLineStatus;
				newMatchingLine.WE_P9_Task = transferLineToTransferPickLineTo.WE_P9_Task;
				newPickLine.WZ_WE_TransactionLine = newMatchingLine.PK;
			}

			return newMatchingLine;
		}

		#endregion

		#endregion

		#region FinaliseTransferLines

		void FinaliseTransferLines(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsTransferLine[] transferLinesToFinalise, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, bool isStrictLinePutaway, Guid transferTaskPK)
		{
			if (transferLinesToFinalise.Any())
			{
				PopulateTransferLinesForPutAway(transfer, transferLineInfo, transferLinesToFinalise);
				transfer.RunPreSaveValidation();
				if (transfer.HasErrors)
				{
					response.LogBusinessValidationError(GetErrorMessageForDocket(transfer));
				}
				else
				{
					var transferLinesWithTheSameTaskPK = transfer.Lines
						.Where(line => transferTaskPK == Guid.Empty || line.WE_P9_Task == transferTaskPK)
						.Cast<WhsTransferLine>()
						.ToArray();
					FinaliseTransferLinesCore(response, transfer, transferLinesWithTheSameTaskPK, transferLinesToFinalise);
					ValidateFinaliseResults(response, transfer, transferLinesWithTheSameTaskPK, transferLinesToFinalise, isAttributesSet, isStrictLinePutaway);
				}
			}
		}

		#region PopulateTransferLinesForPutAway

		void PopulateTransferLinesForPutAway(WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, IEnumerable<WhsTransferLine> allMatchingTransferLines)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			foreach (var transferLine in allMatchingTransferLines)
			{
				PutAwayTransferLine(transferLine, transfer.WD_WW_Whs, transfer.WD_OH_Client, staff, transferLineInfo.DestLocation, transferLineInfo.DestPalletID);
			}
		}

		#endregion

		#region FinaliseTransferLinesCore

		void FinaliseTransferLinesCore(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsTransferLine[] transferLinesWithTheSameTaskPK, IEnumerable<WhsTransferLine> matchingTransferLines)
		{
			transfer.ValidateAndFinaliseDocketLines(matchingTransferLines, false);
			response.IsAllTransferLinesTransferredOrFinalised = !transfer.HasAtLeastOneUnFinalisedTransferLine(transferLinesWithTheSameTaskPK);
		}

		#endregion

		#region ValidateFinaliseResults

		void ValidateFinaliseResults(TransferPutawayWebServiceResponse response, WhsTransfer transfer, WhsTransferLine[] transferLinesWithTheSameTaskPK, IReadOnlyCollection<WhsTransferLine> transferLinesToFinalise, bool isAttributesSet, bool isStrictLinePutaway)
		{
			var notFinalisedTransferLines = transferLinesToFinalise.Where(l => !l.IsFinalised).ToArray();
			ValidateDestinationLocation(response, notFinalisedTransferLines, transferLinesToFinalise);
			ValidateDestinationPalletID(response, notFinalisedTransferLines, transferLinesToFinalise);
			ValidateFullPalletTransfer(response, transfer, notFinalisedTransferLines);
			ValidateSingleProductTransfer(response, transfer, notFinalisedTransferLines, transferLinesToFinalise, isAttributesSet, isStrictLinePutaway);

			response.IsAllTransferLinesTransferredOrFinalised = (response.IsSingleProductTransferred || response.IsFullPalletIDTransferred) &&
				transferLinesWithTheSameTaskPK.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised);
		}

		#region ValidateDestinationLocation

		void ValidateDestinationLocation(TransferPutawayWebServiceResponse response, IReadOnlyCollection<WhsTransferLine> notFinalisedTransferLines, IReadOnlyCollection<WhsTransferLine> transferLinesToFinalise)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var wasAtLeastOneTransferLineFinalised = (notFinalisedTransferLines.Count != transferLinesToFinalise.Count);
				if (wasAtLeastOneTransferLineFinalised)
				{
					response.IsValidDestLocation = true;
				}
				else
				{
					var errorMessage = GetErrorMessageForProperty(notFinalisedTransferLines, WhsDocketLine.Schema.LocationString);
					response.IsValidDestLocation = string.IsNullOrEmpty(errorMessage);
					if (!response.IsValidDestLocation)
					{
						response.LogBusinessValidationError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region ValidateDestinationPalletID

		void ValidateDestinationPalletID(TransferPutawayWebServiceResponse response, IReadOnlyCollection<WhsTransferLine> notFinalisedTransferLines, IReadOnlyCollection<WhsTransferLine> transferLinesToFinalise)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var wasAtLeastOneTransferLineFinalised = (notFinalisedTransferLines.Count != transferLinesToFinalise.Count);
				if (wasAtLeastOneTransferLineFinalised)
				{
					response.IsValidDestPalletID = true;
				}
				else
				{
					var errorMessage = GetErrorMessageForProperty(notFinalisedTransferLines, WhsDocketLineSchema.WE_PalletID.Name);
					response.IsValidDestPalletID = string.IsNullOrEmpty(errorMessage);
					if (!response.IsValidDestPalletID)
					{
						response.LogBusinessValidationError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region ValidateFullPalletTransfer

		void ValidateFullPalletTransfer(TransferPutawayWebServiceResponse response, WhsTransfer transfer, IReadOnlyCollection<WhsTransferLine> notFinalisedTransferLines)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) && response.IsValidSourcePalletID)
			{
				var hasAllTransferLinesBeenFinalised = (notFinalisedTransferLines.Count == 0);
				if (hasAllTransferLinesBeenFinalised)
				{
					response.IsFullPalletIDTransferred = true;
					Factory.Save();
				}
				else
				{
					response.ErrorMessage = GetErrorMessageForDocket(transfer);
				}
			}
		}

		#endregion

		#region ValidateSingleProductTransfer

		void ValidateSingleProductTransfer(TransferPutawayWebServiceResponse response, WhsTransfer transfer, IReadOnlyCollection<WhsTransferLine> notFinalisedTransferLines, IEnumerable<WhsTransferLine> transferLinesToFinalise, bool isAttributesSet, bool isStrictLinePutaway)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) && response.IsValidProduct && response.IsValidInventoryHeldCode && response.IsValidAttributes && isAttributesSet)
			{
				response.IsSingleProductTransferred = !notFinalisedTransferLines.Any();
				if (response.IsSingleProductTransferred)
				{
					response.TotalQuantityTransferred = transferLinesToFinalise.Sum(l => l.QtyToMoveIncludingMatchingLines);
					Factory.Save();
				}
				else
				{
					response.ErrorMessage = GetErrorMessageForDocket(transfer);
				}
			}
			else if (isStrictLinePutaway)
			{
				response.IsSingleProductTransferred = true;
				response.ErrorMessage = null;
				response.TotalQuantityTransferred = transferLinesToFinalise.Sum(l => l.QtyToMoveIncludingMatchingLines);
				Factory.Save();
			}
		}

		#endregion

		string GetErrorMessageForDocket(WhsDocket docket)
		{
			var errorMessage = new ZStringBuilder();
			var messageList = docket.GetErrors().GetUniqueMessageList().Select(e => e.Split(':')[e.Split(':').Length - 1].Trim()).Distinct();
			messageList.ForEach(e => errorMessage.AppendLine(e));
			return errorMessage.ToStringWithNewLineBetweenAppends().Trim();
		}

		#region GetErrorMessageForProperty

		string GetErrorMessageForProperty(IEnumerable<WhsTransferLine> notFinalisedTransferLines, string propertyName)
		{
			string errorMessage = null;
			foreach (var transferLine in notFinalisedTransferLines)
			{
				var info = transferLine.FindPropertyInfo(propertyName);
				if (info.HasErrors())
				{
					errorMessage = info.GetErrors().ToMessageListString();
					break;
				}
			}
			return errorMessage;
		}

		#endregion

		#endregion

		void ValidateDocketLineInfo(TransferPutawayWebServiceResponse response, WhsDocketLineInfo docketLineInfo)
		{
			var validationError = WarehouseValidationHelper.ValidatePalletID(docketLineInfo.DestPalletID);
			if (!string.IsNullOrEmpty(validationError))
			{
				response.LogError(ErrorTypes.BusinessValidationError, validationError);
			}
		}

		#endregion

		#endregion
	}

#if DEBUG
	partial class WhsSecureService
	{
		partial void ThrowExceptionForDataIntegrityTestDuringTransferPutaway()
		{
			CreateTestDataForIntergrityTestDuringTransferPutaway?.Invoke(this, null);
		}
		public EventHandler CreateTestDataForIntergrityTestDuringTransferPutaway;

		partial void InvalidateTransferDataForTest(WhsTransfer transfer)
		{
			CreateInvalidTransferDataForTest?.Invoke(transfer);
		}
		public Action<WhsTransfer> CreateInvalidTransferDataForTest;
	}
#endif
}
