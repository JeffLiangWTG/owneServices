using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region TryToAllocateStockForTransfer

		[WebMethod(Description = "If all required data entered then Allocate stock to a Transfer, otherwise validate available data.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public TransferAllocateWebServiceResponse TryToAllocateStockForTransfer(Guid transferPK, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			return HandleWebServiceRequest<TransferAllocateWebServiceResponse>(r => TryToAllocateStockForTransferCore(r, transferPK, transferLineInfo, isAttributesSet, transferTaskPK));
		}

		void TryToAllocateStockForTransferCore(TransferAllocateWebServiceResponse response, Guid transferPK, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK)
		{
			var transfer = GetValidatedTransferFromTransferPK(Factory, response, transferPK);
			if (response.NoError())
			{
				//	if transfer get finalised after above checking, in warehouse.transactions concurrency handler will check again on Factory Save (handler.PreventSaveIfCriticalValidationsFail)
				var transferLine = transferLineInfo.PK != Guid.Empty
					? Factory.Load<WhsTransferLine>(new ZGuid(transferLineInfo.PK))
					: null;

				var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				if (transferLine != null)
				{
					TryToAllocateStockForSpecifiedTransferLine(response, transferLineInfo, transferTaskPK, transfer, transferLine, staff);
				}
				else
				{
					TryToAllocateStockForMatchedTransferLines(response, transferLineInfo, isAttributesSet, transferTaskPK, transfer, staff);
				}

				response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
			}
		}

		static WhsTransfer GetValidatedTransferFromTransferPK(BusinessObjectFactory factory, TransferAllocateWebServiceResponse response, Guid transferPK)
		{
			var transfer = transferPK != Guid.Empty
				? factory.Load<WhsTransfer>(transferPK)
				: null;

			if (transfer != null)
			{
				if (transfer.IsFinalised)
				{
					response.LogBusinessValidationError(Res.GetString("3339f8c7-1052-4c57-9c09-cdb390dfa662", "Transfer record has been finalized."));
				}
				else
				{
					transfer.CheckTransferIsMasterTransfer(response);
				}
			}

			return transfer;
		}

		#region TryToAllocateStockForSpecifiedTransferLine

		void TryToAllocateStockForSpecifiedTransferLine(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo, Guid transferTaskPK, WhsTransfer transfer, WhsTransferLine transferLine, GlbStaff staff)
		{
			if (transferLine.IsPicked)
			{
				response.LogBusinessValidationError(Res.GetString("6d6b2207-5dc3-47b8-a4a1-9616d13bf4cb", "Transfer Line is already Picked."));
			}
			else if (transferTaskPK != Guid.Empty && transferLine.WE_P9_Task != transferTaskPK)
			{
				response.LogBusinessValidationError(TransferLineAssignedToAnotherUser);
			}
			else
			{
				var linePicked = AllocateStockForExistingLine(response, transfer, transferLine, transferLineInfo.Qty, staff);
				UpdateTransferAllocateWebserviceResponse(response, transfer, linePicked);
			}
		}

		void UpdateTransferAllocateWebserviceResponse(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsTransferLine linePicked)
		{
			SaveTransferUpdates(transfer, response);
			if (response.NoError())
			{
				response.Transfer = new WhsDocketInfo(transfer);
				response.NewTransferLines = new WhsDocketLineInfoCollection { new WhsDocketLineInfo(linePicked) };
			}
		}

		#endregion

		#region TryToAllocateStockForMatchedTransferLines

		void TryToAllocateStockForMatchedTransferLines(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo, bool isAttributesSet, Guid transferTaskPK, WhsTransfer transfer, GlbStaff staff)
		{
			var matchingTransferLines = transfer != null
				? transfer.Lines.Cast<WhsTransferLine>().Where(l => !l.IsPicked && MatchTransferLine(l, transferLineInfo)).ToArray()
				: Array.Empty<WhsTransferLine>();

			if (matchingTransferLines.Any())
			{
				AllocateStockForExistingLines(response, transferTaskPK, transfer, matchingTransferLines, staff);
			}
			else
			{
				var matchingInventories = GetAllMatchingInventory(response, transfer, transferLineInfo, isAttributesSet);
				AllocateStockForTransfer(response, transfer, transferLineInfo, matchingInventories, transferTaskPK, staff);
			}
		}

		bool MatchTransferLine(WhsTransferLine transferLine, WhsDocketLineInfo transferLineInfo)
		{
			var transferFromLocation = transferLine.TransferFromLocation;
			return WhsLocationHelper.LocationStringCompare(transferFromLocation, transferLineInfo.Location) &&
					transferLine.WE_TransferFromPalletId.EqualsIgnoringCase(transferLineInfo.PalletID) &&
					transferLine.WE_OP == transferLineInfo.Product.PK &&
					transferLine.WE_PackingDate.CompareTo(new ZDateTime(transferLineInfo.PackingDate)) == 0 &&
					transferLine.WE_ExpiryDate.CompareTo(new ZDateTime(transferLineInfo.ExpiryDate)) == 0 &&
					transferLine.WE_PartAttrib1.EqualsIgnoringCase(transferLineInfo.Attribute1) &&
					transferLine.WE_PartAttrib2.EqualsIgnoringCase(transferLineInfo.Attribute2) &&
					transferLine.WE_PartAttrib3.EqualsIgnoringCase(transferLineInfo.Attribute3) &&
					transferLine.WE_SerialNumber.EqualsIgnoringCase(transferLineInfo.SerialNumber);
		}

		void AllocateStockForExistingLines(TransferAllocateWebServiceResponse response, Guid transferTaskPK, WhsTransfer transfer, WhsTransferLine[] matchingTransferLines, GlbStaff staff)
		{
			var transferLinesWithMatchingTask = transferTaskPK == Guid.Empty
				? matchingTransferLines
				: matchingTransferLines.Where(line => line.WE_P9_Task == transferTaskPK).ToArray();

			if (transferLinesWithMatchingTask.Length == 0)
			{
				response.LogBusinessValidationError(TransferLineAssignedToAnotherUser);
			}
			else
			{
				AllocateStockForMatchingTransferLines(response, transfer, transferLinesWithMatchingTask, staff);
			}
		}

		void AllocateStockForMatchingTransferLines(TransferAllocateWebServiceResponse response, WhsTransfer transfer, IEnumerable<WhsTransferLine> matchingTransferLines, GlbStaff staff)
		{
			var pickedTransferLines = new List<WhsTransferLine>();

			foreach (var matchingTransferLine in matchingTransferLines)
			{
				var line = AllocateStockForExistingLine(response, transfer, matchingTransferLine, matchingTransferLine.QtyToMoveIncludingMatchingLines, staff);
				if (line.IsPicked)
				{
					pickedTransferLines.Add(line);
				}
			}

			SaveTransferUpdates(transfer, response);
			if (response.NoError())
			{
				response.Transfer = new WhsDocketInfo(transfer);
				response.PickedTransferLinePks = pickedTransferLines.Select(l => l.PK.ToGuid()).ToArray();
				response.TotalPickLineQuantity = pickedTransferLines.Sum(t => t.QtyToMoveIncludingMatchingLines);
			}
		}

		#endregion

		#region AllocateStockForExistingLine

		WhsTransferLine AllocateStockForExistingLine(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsTransferLine transferLine, decimal pickLineQuantity, GlbStaff staff)
		{
			response.TotalPickLineQuantity = pickLineQuantity;

			var linePicked = SplitTransferLineIfRequired(transfer, transferLine, pickLineQuantity);
			linePicked.GS_NKPickedBy = staff.GS_Code;
			linePicked.PickedTime = ZDateTimeOffset.Now;

			return linePicked;
		}

		#endregion

		#region GetAllMatchingInventory

		IReadOnlyList<WhsInventoryView> GetAllMatchingInventory(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, bool isAttributesSet)
		{
			var matchingInventories = FindAvailableInventoryByLocationOrPalletID(response, transferLineInfo);
			FindAvailableInventoryByProduct(response, transfer, transferLineInfo, matchingInventories, out OrgSupplierPart part);
			ValidateIfPalletIDRequired(response, matchingInventories);
			FindAvailableInventoryByInventoryHeldCode(response, transferLineInfo, matchingInventories);
			ValidateQuantity(response, transferLineInfo, matchingInventories);

			FindAvailableInventoryByAttributes(response, transferLineInfo, matchingInventories, isAttributesSet, part);

			return matchingInventories;
		}

		#region FindAvailableInventoryByLocationOrPalletID

		List<WhsInventoryView> FindAvailableInventoryByLocationOrPalletID(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo)
		{
			var matchingInventories = new List<WhsInventoryView>();
			if (!string.IsNullOrEmpty(transferLineInfo.Location) || !string.IsNullOrEmpty(transferLineInfo.PalletID))
			{
				var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, transferLineInfo.Location, transferLineInfo.PalletID);
				var inventoryLoader = new WhsInventoryLoader(Factory, SecurityHeader.WarehouseCode);
				var loadedInventory = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And);
				matchingInventories.AddRange(loadedInventory
					.Where(i => i.WI_AvailableToTransferQuantity > 0m && WebServiceHelper.InventoryAllowsTransfer(i, i.Location))); //remove dockdoor located and Putaway transfer 'Putaway' inventory

				if (matchingInventories.Any())
				{
					response.IsValidLocation = !string.IsNullOrEmpty(transferLineInfo.Location);
					response.IsValidPalletID = !string.IsNullOrEmpty(transferLineInfo.PalletID);
				}
				else
				{
					if (!string.IsNullOrEmpty(transferLineInfo.Location))
					{
						response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("2496498d-4452-4c95-8c04-9418cbd7d7d7", "No stock can be transferred from this Location."));
					}
					else
					{
						response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("13d2b431-389f-47ad-9323-c2fd85020f9c", "No stock can be transferred from this Pallet ID."));
					}
				}
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("b1ec4696-3c1b-4aa5-8733-300975e52814", "Please enter a Location or Pallet ID to transfer inventory from."));
			}

			return matchingInventories;
		}

		#endregion

		#region FindAvailableInventoryByProduct

		void FindAvailableInventoryByProduct(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, List<WhsInventoryView> matchingInventories, out OrgSupplierPart part)
		{
			part = null;
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var partPK = (transferLineInfo.Product != null) ? new ZGuid(transferLineInfo.Product.PK) : ZGuid.Empty;
				if (partPK != ZGuid.Empty)
				{
					var client = GetClient(transfer, transferLineInfo.ClientCode);
					if (client != null)
					{
						part = WebServiceHelper.GetPartByPartNum(Factory, transferLineInfo.Product.Code, client.PK);
						if (part != null && part.OP_IsActive)
						{
							matchingInventories.RemoveAll(i => i.WI_OP != partPK || i.WI_OH_Client != client.PK);
							response.IsValidProduct = matchingInventories.Any();
							if (!response.IsValidProduct)
							{
								response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("f35f2772-4657-4f4f-8824-4e4f6d53829f", "No inventory of this Product is available to transfer."));
							}
						}
						else
						{
							if (part == null)
							{
								response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("83b8156b-1656-464f-b7b2-a6797972546d", "Product could not be found for client: {0}. Please provide a valid product code or barcode.", client.OH_Code));
							}
							else
							{
								response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("ba03845b-2433-4512-89f8-fb5307ecfcd1", "Stock was found with Inactive Product, Set Product to Active for use."));
							}
						}
					}
					else
					{
						response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("285e71a3-eb1b-413b-ac75-6ec3c743b5df", "No client is specified for this transfer."));
					}
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("d21efa54-e6c6-45a1-954b-57f94ddc6d33", "Please provide a valid product code or barcode."));
				}
			}
		}

		OrgHeader GetClient(WhsTransfer transfer, string clientCode)
		{
			OrgHeader result = null;
			if (transfer != null)
			{
				result = transfer.Client;
			}
			else if (!string.IsNullOrEmpty(clientCode))
			{
				result = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, clientCode);
			}
			return result;
		}

		#endregion

		#region ValidateIfPalletIDRequired

		void ValidateIfPalletIDRequired(TransferAllocateWebServiceResponse response, List<WhsInventoryView> matchingInventories)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) && response.IsValidLocation)
			{
				var allPalletIDs = matchingInventories.Select(i => i.WI_PalletID).Distinct().ToArray();
				var hasEmptyPalletIDs = allPalletIDs.Contains("");
				if (hasEmptyPalletIDs)
				{
					matchingInventories.RemoveAll(i => !i.WI_PalletID.IsEmpty);
				}
				response.IsValidLocation = (hasEmptyPalletIDs || allPalletIDs.Length == 1);
				if (!response.IsValidLocation)
				{
					response.IsValidLocation = false;
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("ca921942-ed18-41a6-be89-5f5d92bed514", "This product exist on multiple pallets in this location. Please scan a Pallet ID that you want to transfer."));
				}
			}
		}

		#endregion

		#region FindAvailableInventoryByInventoryHeldCode

		void FindAvailableInventoryByInventoryHeldCode(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo, List<WhsInventoryView> matchingInventories)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				matchingInventories.RemoveAll(i => i.WI_HeldCode != transferLineInfo.InventoryHeldCode);
				response.IsValidInventoryHeldCode = matchingInventories.Any();
				if (!response.IsValidInventoryHeldCode)
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("4f859f44-d003-41f7-92d7-2f901777dd08", "No inventory with Hold Code '{0}' available to Transfer.", transferLineInfo.InventoryHeldCode));
				}
			}
		}

		#endregion

		#region ValidateQuantity

		void ValidateQuantity(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo, IReadOnlyList<WhsInventoryView> matchingInventories)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.TotalQuantityAvailableToPick = matchingInventories.Sum(i => i.WI_AvailableToTransferQuantity);
				if (response.TotalQuantityAvailableToPick < transferLineInfo.Qty)
				{
					var list = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPairWithStandardUnits(SecurityHeader.IsAndroidDevice, Factory);
					var firstInventory = matchingInventories[0];
					var unitUQDesc = list.GetDescriptionFromCode(firstInventory.WI_UnitsUQ);
					var errorMessage = Res.GetString("8ba6c437-b713-4751-aa13-d146983f03a2", "Cannot pick {0} {2} only {1} {2} available to transfer.", transferLineInfo.Qty.ToString("G29", CultureInfo.InvariantCulture),
						response.TotalQuantityAvailableToPick.ToString("G29", CultureInfo.InvariantCulture), // http://stackoverflow.com/questions/4525854/remove-trailing-zeros
						unitUQDesc ?? firstInventory.WI_UnitsUQ);

					response.LogError(ErrorTypes.BusinessValidationError, errorMessage);
				}
			}
		}

		#endregion

		#region FindAvailableInventoryByAttributes

		void FindAvailableInventoryByAttributes(TransferAllocateWebServiceResponse response, WhsDocketLineInfo transferLineInfo, List<WhsInventoryView> matchingInventories, bool isAttributesSet, OrgSupplierPart part)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				if (isAttributesSet)
				{
					matchingInventories.RemoveAll(i =>
						!i.WI_PartAttrib1.EqualsIgnoringCase(transferLineInfo.Attribute1) ||
						!i.WI_PartAttrib2.EqualsIgnoringCase(transferLineInfo.Attribute2) ||
						!i.WI_PartAttrib3.EqualsIgnoringCase(transferLineInfo.Attribute3) ||
						!i.WI_SerialNumber.EqualsIgnoringCase(transferLineInfo.SerialNumber) ||
						!i.WI_ExpiryDate.Equals(transferLineInfo.ExpiryDate.Date) ||
						!i.WI_PackingDate.Equals(transferLineInfo.PackingDate.Date));

					response.IsValidAttributes = (matchingInventories.Any());
					if (!response.IsValidAttributes)
					{
						response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("5ed3b9d9-4c39-46b8-9dab-ab8d42fdedd7", "None of the stock available to transfer match entered attributes."));
					}
				}
				else
				{
					var firstInventory = matchingInventories.FirstOrDefault();
					var partRelation = (part != null && firstInventory != null) ? part.RelatedOrganisations.FindFirstByOrganisationPK(firstInventory.WI_OH_Client) : null;
					if (firstInventory != null && partRelation != null && partRelation.OU_RFAttributeConfirm == RFAttributeConfirmCode.Codes.None)
					{
						response.IsValidAttributes =
							transferLineInfo.Qty == response.TotalQuantityAvailableToPick ||
							(firstInventory != null && matchingInventories.All(i => AttributeComparer.Compare(i, firstInventory)));
					}
				}
			}
		}

		#endregion

		#endregion

		#region AllocateStockForTransfer

		void AllocateStockForTransfer(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, IReadOnlyList<WhsInventoryView> matchingInventories, Guid transferTaskPK, GlbStaff staff)
		{
			if (string.IsNullOrEmpty(response.ErrorMessage) &&
				(response.IsValidLocation || response.IsValidPalletID) &&
				response.IsValidProduct &&
				response.IsValidInventoryHeldCode &&
				response.TotalQuantityAvailableToPick >= transferLineInfo.Qty &&
				response.IsValidAttributes)
			{
				ProcessTask transferTask = null;
				if (transfer == null)
				{
					if (transferTaskPK != Guid.Empty)
					{
						response.LogBusinessValidationError(Res.GetString("22b1c5e9-0811-4666-ab21-778fd4688871", "Task started with no transfer started yet."));
					}
					else
					{
						(transfer, transferTask) = CreateTransferAndAssociatedTaskIfNecessary(matchingInventories[0].WI_OH_Client, staff);
						if (transferTask != null)
						{
							BeginRFTaskHelper.BeginRFTask(response, transferTask, WarehouseTaskFormFlowTypes.TransferJob, staff);
						}
					}
				}

				var taskPK = transferTask?.PK.ToGuid() ?? transferTaskPK;
				if (response.NoError())
				{
					var newTransferLines = CreateTransferLines(response, transfer, transferLineInfo, matchingInventories, taskPK, staff);
					SaveTransferUpdates(transfer, response);

					if (response.NoError())
					{
						response.Transfer = new WhsDocketInfo(transfer) { TaskPK = taskPK };
						response.NewTransferLines = new WhsDocketLineInfoCollection(newTransferLines);
					}
				}
			}
		}

		(WhsTransfer, ProcessTask) CreateTransferAndAssociatedTaskIfNecessary(ZGuid clientPK, GlbStaff staff)
		{
			ProcessTask task = null;
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var transfer = Factory.New<WhsTransfer>();
			transfer.WD_OH_Client = clientPK;
			transfer.WD_WW_Whs = warehouse.PK;

			if (warehouse.IsTaskManagementEnabled)
			{
				task = CreateTransferProcessTask(Factory, transfer, WarehouseTaskFormFlowTypes.TransferJob, staff);
			}
			return (transfer, task);
		}

		IEnumerable<WhsDocketLine> CreateTransferLines(TransferAllocateWebServiceResponse response, WhsTransfer transfer, WhsDocketLineInfo transferLineInfo, IEnumerable<WhsInventoryView> matchingInventories, Guid transferTaskPK, GlbStaff staff)
		{
			var dictionary = new Dictionary<string, WhsTransferLine>();
			var unitsStillToPick = transferLineInfo.Qty;
			foreach (var inventory in matchingInventories)
			{
				var key = AttributeComparer.GetHashCodeForConsolidation(inventory);
				if (!dictionary.TryGetValue(key, out WhsTransferLine transferLine))
				{
					transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
					transferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;
					transferLine.WE_F3_NKPackType = transferLineInfo.PackUQ;
					transferLine.QtyToMoveIncludingMatchingLines = 0m;
					transferLine.GS_NKPickedBy = staff.GS_Code;
					transferLine.WE_P9_Task = transferTaskPK;

					dictionary.Add(key, transferLine);
				}
				var qty = Math.Min(unitsStillToPick, inventory.WI_AvailableToTransferQuantity);
				transferLine.QtyToMoveIncludingMatchingLines += qty;
				unitsStillToPick -= qty;
				if (unitsStillToPick <= 0)
				{
					break;
				}
			}

			var now = ZDateTimeOffset.Now;
			foreach (var transferLine in dictionary.Values)
			{
				transferLine.PickedTime = now; // Commit and Pick stock.
			}

			response.TotalPickLineQuantity = transferLineInfo.Qty - Math.Max(unitsStillToPick, 0);
			return dictionary.Values;
		}

		#endregion

		#endregion

		void SaveTransferUpdates(WhsTransfer transfer, TransferAllocateWebServiceResponse response)
		{
			transfer.RunPreSaveValidation(); // to commit stock.
			if (transfer.HasErrors)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = transfer.GetErrors().ToUniqueMessageListString();
			}
			else
			{
				var unSubscribeServiceOnFactorySaving = false;
				if (!transfer.IsInDatabase)
				{
					Factory.Saving += SubscribeServiceOnFactorySaving;
					unSubscribeServiceOnFactorySaving = true;
				}

				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => TransferConcurrencyErrorMessage);
				
				if (unSubscribeServiceOnFactorySaving)
				{
					Factory.Saving -= SubscribeServiceOnFactorySaving;
				}
			}
		}

		static string TransferLineAssignedToAnotherUser => Res.GetString("c0c7733f-11e3-4229-a476-c181c686c0c1", "Transfer Line is assigned to another user.");
	}
}
