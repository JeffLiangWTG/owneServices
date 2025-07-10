using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CreateNewWhsTransfers

		[WebMethod(Description = "Create New Warehouse Transfers")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public WhsTransfersWebServiceResponse CreateNewWhsTransfers(string sourceLocationOrPalletID, string clientCode = "")
		{
			return HandleWebServiceRequest<WhsTransfersWebServiceResponse>(result =>
			{
				ValidateAndCreateTransfers(result, sourceLocationOrPalletID, clientCode);
				result.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
			});
		}

		void ValidateAndCreateTransfers(WhsTransfersWebServiceResponse response, string sourceLocationOrPalletID, string clientCode)
		{
			var location = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, sourceLocationOrPalletID);
			if (response.NoError())
			{
				var criteria = (location == null)
					? new WhsInventorySearchCriteriaInfo(clientCode, string.Empty, string.Empty, sourceLocationOrPalletID)
					: new WhsInventorySearchCriteriaInfo(clientCode, string.Empty, sourceLocationOrPalletID, string.Empty);
				var inventoryLoader = new WhsInventoryLoader(Factory, SecurityHeader.WarehouseCode);
				var availableInventories = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And);

				var stockStatus = GetStockAvailabilityStatus(availableInventories);
				var errorMessage = ValidateStockAvailability(stockStatus, sourceLocationOrPalletID, clientCode);
				if (string.IsNullOrEmpty(errorMessage))
				{
					response.IsStockCommittedOrReserved = stockStatus == StockAvailabilityStatus.SomeCommittedOrReservedOrPutaway;
					response.SourceLocation = (availableInventories.Length > 0) ? availableInventories[0].Location?.WLV_LocationString ?? ZString.Empty : null;

					if (stockStatus == StockAvailabilityStatus.AllAvailable)
					{
						var transfers = CreateWhsTransfers(availableInventories, response);
						if (!transfers.Any())
						{
							response.LogBusinessValidationError(Res.GetString("0b772c91-0dc6-4e33-a656-5b2b0dba1c41", "All stock in this location is committed or reserved and cannot be transferred."));
						}
						else
						{
							response.Transfers = transfers;
						}
					}
				}
				else
				{
					response.LogBusinessValidationError(errorMessage);
				}
			}
		}

		#region GetStockAvailabilityStatus

		StockAvailabilityStatus GetStockAvailabilityStatus(IEnumerable<WhsInventoryView> availableInventories)
		{
			var isAvailableStockFound = false;
			var isCommittedOrReservedOrPutawayFound = false;
			var isProductActive = true;

			foreach (var inventory in availableInventories)
			{
				if (!isAvailableStockFound)
				{
					var location = inventory.Location;
					if (location != null && WebServiceHelper.InventoryAllowsTransfer(inventory, location) && inventory.WI_AvailableToTransferQuantity > 0m)
					{
						isAvailableStockFound = true;
					}
				}

				if (!isCommittedOrReservedOrPutawayFound &&
						(
							inventory.CommittedQuantityIncludingUnfinalisedReceipt > 0m ||
							inventory.WI_CrossDockQuantity > 0m ||
							inventory.WI_InventoryStatus == InventoryStatus.Codes.Putaway
						)
					)
				{
					isCommittedOrReservedOrPutawayFound = true;
				}

				isProductActive = inventory.Product.Parent.OP_IsActive;
				if ((isAvailableStockFound && isCommittedOrReservedOrPutawayFound) || !isProductActive)
				{
					break;
				}
			}

			var result = GetStockAvailabilityFlag(isAvailableStockFound, isCommittedOrReservedOrPutawayFound, isProductActive);
			return result;
		}

		static StockAvailabilityStatus GetStockAvailabilityFlag(bool isAvailableStockFound, bool isCommittedOrReservedStockFound, bool isProductActive)
		{
			StockAvailabilityStatus result;
			if (isProductActive)
			{
				if (isAvailableStockFound)
				{
					result = isCommittedOrReservedStockFound ? StockAvailabilityStatus.SomeCommittedOrReservedOrPutaway : StockAvailabilityStatus.AllAvailable;
				}
				else
				{
					result = isCommittedOrReservedStockFound ? StockAvailabilityStatus.AllCommittedOrReservedOrPutaway : StockAvailabilityStatus.NoStockFound;
				}
			}
			else
			{
				result = StockAvailabilityStatus.StockIsInactive;
			}
			return result;
		}

		enum StockAvailabilityStatus
		{
			AllAvailable,
			SomeCommittedOrReservedOrPutaway,
			AllCommittedOrReservedOrPutaway,
			NoStockFound,
			StockIsInactive
		}

		#endregion

		#region ValidateStockAvailability

		string ValidateStockAvailability(StockAvailabilityStatus stockStatus, string sourceLocationOrPalletID, string clientCode)
		{
			string clientCodeMessage = string.IsNullOrEmpty(clientCode) ? "" : " " + Res.GetString("876f9591-c7aa-4de2-b833-1680cc91a122", "for Client '{0}'", clientCode);

			switch (stockStatus)
			{
				case StockAvailabilityStatus.NoStockFound:
					return Res.GetString("8475ec8d-a9e2-4c5a-8ede-586f14da18e1", "No stock found to transfer from Location Or Pallet ID '{0}'{1}.", sourceLocationOrPalletID, clientCodeMessage);
				case StockAvailabilityStatus.AllCommittedOrReservedOrPutaway:
					return Res.GetString("d3533f47-6e72-46a7-9a6e-4c7f768bc243", "All stock in this location is committed, reserved or putaway and cannot be transferred.") + clientCodeMessage;
				case StockAvailabilityStatus.StockIsInactive:
					return Res.GetString("ba03845b-2433-4512-89f8-fb5307ecfcd1", "Stock was found with Inactive Product, Set Product to Active for use.");
				default:
					return "";
			}
		}

		#endregion

		#region CreateWhsTransfers

		WhsDocketInfo[] CreateWhsTransfers(IEnumerable<WhsInventoryView> availableInventories, WhsTransfersWebServiceResponse response)
		{
			var transfersAndAssociatedTransferTaskPKsByClient = CreateWhsTransfersAndAssociatedProcessTasks(availableInventories, response);
			var transfersInfoList = new List<WhsDocketInfo>();
			if (transfersAndAssociatedTransferTaskPKsByClient.Any())
			{
				Factory.Saving += SubscribeServiceOnFactorySaving;
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => TransferConcurrencyErrorMessage);
				Factory.Saving -= SubscribeServiceOnFactorySaving;

				var shouldCreateTransferLines = false;
				var transferTaskPKToReturn = transfersAndAssociatedTransferTaskPKsByClient.First().Value.taskPK;
				foreach (var transfer in transfersAndAssociatedTransferTaskPKsByClient.Values.Select(t => t.transfer))
				{
					var transferInfo = new WhsDocketInfo(transfer, shouldCreateTransferLines);
					if (!transferTaskPKToReturn.IsEmpty)
					{
						transferInfo.TaskPK = transferTaskPKToReturn.ToGuid();
					}
					transfersInfoList.Add(transferInfo);
				}
			}

			return transfersInfoList.ToArray();
		}

		Dictionary<ZGuid, (WhsTransfer transfer, ZGuid taskPK)> CreateWhsTransfersAndAssociatedProcessTasks(IEnumerable<WhsInventoryView> availableInventories, WhsTransfersWebServiceResponse response)
		{
			var transfersDictionary = new Dictionary<ZGuid, (WhsTransfer transfer, ZGuid taskPK)>();
			var transferLinesDictionary = new Dictionary<ZString, WhsTransferLine>();
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			foreach (var inventory in availableInventories)
			{
				if (inventory.WI_AvailableToTransferQuantity > 0m)
				{
					var (transfer, taskPK) = GetOrCreateTransferAndRelatedProcessTask(transfersDictionary, warehouse, inventory, staff, response);
					UpdateOrCreateTransferLine(transfer, transferLinesDictionary, inventory, staff, taskPK);
				}
			}

			var now = ZDateTimeOffset.Now;

			foreach (var transfer in transfersDictionary.Values.Select(t => t.transfer))
			{
				transfer.RunPreSaveValidation(); // to commit stock.

				// Pick all stock
				foreach (WhsTransferLine transferLine in transfer.Lines)
				{
					transferLine.PickedTime = now;
				}
			}

			return transfersDictionary;
		}

		#region GetOrCreateTransfer

		(WhsTransfer, ZGuid) GetOrCreateTransferAndRelatedProcessTask(Dictionary<ZGuid, (WhsTransfer, ZGuid)> transfersDictionary, WhsWarehouse warehouse, WhsInventoryView inventory, GlbStaff staff, WhsTransfersWebServiceResponse response)
		{
			var transfersDictionaryHasTransfers = transfersDictionary.Count > 0;
			if (!transfersDictionary.TryGetValue(inventory.WI_OH_Client, out (WhsTransfer transfer, ZGuid taskPK) transferAndTaskPK))
			{
				transferAndTaskPK.transfer = CreateWhsTransfer(warehouse.PK, inventory.WI_OH_Client);
				if (warehouse.IsTaskManagementEnabled)
				{
					var task = CreateTransferProcessTask(Factory, transferAndTaskPK.transfer, WarehouseTaskFormFlowTypes.TransferJob, staff);
					if (!transfersDictionaryHasTransfers)
					{
						BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.TransferJob, staff);
					}
					transferAndTaskPK.taskPK = task.PK;
				}
				
				transfersDictionary.Add(inventory.WI_OH_Client, transferAndTaskPK);
			}
			return transferAndTaskPK;
		}

		WhsTransfer CreateWhsTransfer(ZGuid warehousePK, ZGuid clientPK)
		{
			WhsTransfer transfer = Factory.New<WhsTransfer>();
			transfer.WD_OH_Client = clientPK;
			transfer.WD_WW_Whs = warehousePK;
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			return transfer;
		}

		#endregion

		#region UpdateOrCreateTransferLine

		WhsTransferLine UpdateOrCreateTransferLine(WhsTransfer transfer, Dictionary<ZString, WhsTransferLine> transferLinesDictionary, WhsInventoryView inventory, GlbStaff staff, ZGuid taskPK)
		{
			var key = GetInventoryKey(inventory);
			if (!transferLinesDictionary.TryGetValue(key, out WhsTransferLine transferLine))
			{
				transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
				transferLine.WE_P9_Task = taskPK;
				transferLinesDictionary.Add(key, transferLine);
			}
			else
			{
				transferLine.QtyToMoveIncludingMatchingLines += inventory.WI_AvailableToTransferQuantity;
			}
			transferLine.GS_NKPickedBy = staff.GS_Code;

			return transferLine;
		}

		ZString GetInventoryKey(WhsInventoryView inventory)
		{
			var separator = "*";
			var builder = new ZStringBuilder();
			builder.Append(inventory.WI_OH_Client.ToString());
			builder.Append(inventory.WI_OP.ToString());
			builder.Append(inventory.WI_ArrivalDate.ToShortDateString());
			builder.Append(inventory.WI_WL.ToString());
			builder.Append(inventory.WI_PalletID);
			builder.Append(inventory.WI_InventoryStatus);
			builder.Append(inventory.WI_HeldCode);
			builder.Append(inventory.WI_PartAttrib1);
			builder.Append(inventory.WI_PartAttrib2);
			builder.Append(inventory.WI_PartAttrib3);
			builder.Append(inventory.WI_SerialNumber);
			builder.Append(inventory.WI_PackingDate.ToShortDateString());
			builder.Append(inventory.WI_ExpiryDate.ToShortDateString());
			return builder.ToStringWithDelimiterBetweenAppends(separator);
		}

		#endregion

		#region SubscribeServiceOnFactorySaving

		void SubscribeServiceOnFactorySaving(BusinessObjectFactory f)
		{
			f.Saving -= SubscribeServiceOnFactorySaving;
			f.ServiceContainer.AddAfterOnSavingService(new AddEventsForTransferService());
		}

		class AddEventsForTransferService : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				var transfers = businessObjectsInOnSavingOrder.OfType<WhsTransfer>();
				transfers.ForEach(t => t.AddEvents(ZArchitecture.Business.Events.ServiceCommenced));
			}
		}

		#endregion

		#endregion

		#endregion

		static string TransferConcurrencyErrorMessage => Res.GetString("f77a2563-1ffa-4bdb-946b-c9e310ca822a", "Another user has changed the transfer job while you have been working on it. Please restart the operation and try again.");
	}
}
