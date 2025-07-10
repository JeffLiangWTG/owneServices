using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Putaway Pallet")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPalletWebServiceResponse PutawayPallet(string palletID, string locationString, string putawayPalletID, bool isMultiPalletPutaway, Guid taskPK)
		{
			return HandleWebServiceRequest<WhsPalletWebServiceResponse>(result => PutawayPalletCore(result, palletID, locationString, putawayPalletID, isMultiPalletPutaway, taskPK));
		}

		void PutawayPalletCore(WhsPalletWebServiceResponse response, string palletID, string locationString, string putawayPalletID, bool isMultiPalletPutaway, Guid taskPK)
		{
			var location = !string.IsNullOrEmpty(locationString)
				? WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, locationString)
				: null;

			if (response.NoError())
			{
				CheckLocationAndPutaway(response, palletID, putawayPalletID, location, isMultiPalletPutaway, taskPK);
			}
		}

		void CheckLocationAndPutaway(WhsPalletWebServiceResponse response, string palletID, string putawayPalletID, WhsLocation location, bool isMultiPalletPutaway, ZGuid taskPK)
		{
			ValidateLocationForPutaway(response, location);
			if (response.NoError())
			{
				var inventoryCollection = GetInventory();
				if (inventoryCollection != null)
				{
					var inventoriesAndReceiveLines = GetReceiveLinesForPutaway(inventoryCollection);
					var putawayTransferLinesToFinalise = GetPutawayTransfersToFinalise(inventoriesAndReceiveLines, response, location, putawayPalletID);
					var receiveLines = inventoriesAndReceiveLines.Select(line => line.ReceiveLine).ToArray();

					ValidateInventoryForBondedOrInwardProcessingLocations(response, location, putawayTransferLinesToFinalise);
					CheckIfAllOrNoneAreCrossDocked(response, putawayTransferLinesToFinalise, palletID);
					CheckLocationCapacity(Factory, response, receiveLines, location);
					PutawayPallet(response, receiveLines, putawayTransferLinesToFinalise, palletID, putawayPalletID, isMultiPalletPutaway, taskPK);
				}
			}

			IEnumerable<WhsInventoryView> GetInventory()
			{
				IEnumerable<WhsInventoryView> result = null;

				var inventoryCollection = WebServiceHelper.LoadWhsInventoryByPalletID(response, Factory, SecurityHeader.WarehouseCode, palletID);
				if (response.NoError())
				{
					if (!inventoryCollection.Any())
					{
						inventoryCollection = WebServiceHelper.LoadWhsInventoryByTransferFromPalletID(response, Factory, SecurityHeader.WarehouseCode, palletID, true);
					}

					return response.NoError() ? inventoryCollection : null;
				}

				return result;
			}
		}

		static void CheckLocationCapacity(BusinessObjectFactory factory, WhsPalletWebServiceResponse response, IEnumerable<WhsReceiveLine> receiveLines, WhsLocation location)
		{
			if (response.NoError() && location.WLV_MaxQuantity > 0)
			{
				var qtyGoingToBeMoved = receiveLines.Sum(i => i.WE_TransactionQuantity);
				var remainingCapacity = location.WLV_MaxQuantity - WebServiceHelper.GetConsumedCapacityForLocation(factory, location.PK);
				if (qtyGoingToBeMoved > remainingCapacity)
				{
					response.LogBusinessValidationError(Res.GetString("1D53FB8E-D445-4C7A-8233-555D6E413D7C", "Putaway quantity {0} will exceed current available location capacity {1}", qtyGoingToBeMoved, remainingCapacity));
				}
			}
		}

		static void CheckIfAllOrNoneAreCrossDocked(WhsPalletWebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, string palletID)
		{
			if (response.NoError()
				&& transferLines.Any(line => line.IsCrossDockPutaway)
				&& transferLines.Any(line => !line.IsCrossDockPutaway))
			{
				response.LogBusinessValidationError(Res.GetString("9177e989-7c94-48d7-aa27-def9796b1cd1", "Pallet '{0}' cannot be put away to the cross dock location as some inventories on this pallet are not cross docked.", palletID));
			}
		}

		void PutawayPallet(
			WhsPalletWebServiceResponse response,
			IEnumerable<WhsReceiveLine> receiveLines,
			IEnumerable<WhsTransferLine> putawayTransferLinesToFinalise,
			string palletID,
			string putawayPalletID,
			bool isMultiPalletPutaway,
			ZGuid taskPK)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (response.NoError() && PutawayHelper.AreAllPutawayTransfersUnfinalized(response, putawayTransferLinesToFinalise, palletID))
			{
				var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				ClosePutawayTaskIfAllLinesFinalised(response, warehouse, staff, putawayTransferLinesToFinalise, taskPK);
				PutawayPallet(staff);
			}

			void PutawayPallet(GlbStaff staff)
			{
				var parentTransferLines = putawayTransferLinesToFinalise.Where(l => l.WE_WE_MatchingLine.IsEmpty).ToArray();
				if (parentTransferLines.Length != 0)
				{
					ValidateAndFinaliseDocket(response, parentTransferLines[0].Docket, parentTransferLines);
				}

				if (response.NoError())
				{
					if (!isMultiPalletPutaway)
					{
						var putawayJob = PutawayHelper.CreateFinalisedPutawayJobWithPutawayLine(Factory, warehouse, staff, palletID);
						var putawayLine = putawayJob.Lines.Single(); // only 1 line should be created
						if (putawayTransferLinesToFinalise.Any(tl => !tl.WE_TransferFromPalletId.EqualsIgnoringCase(palletID)))
						{
							response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("3e183431-1123-46f9-baa6-77df240e7399", "All putaway transfer's pallet ID must match scanned pallet ID."));
						}
						else
						{
							putawayTransferLinesToFinalise.ForEach(tl => tl.WE_WPL_PutawayLine = putawayLine.PK);
						}
					}
					else
					{
						FinalisePutawayJobAndLineAsRequired(response, palletID, warehouse, staff);
					}

					if (response.NoError())
					{
						var concurrencyErrorMessage = Res.GetString("d596eac0-596e-4e3d-be45-c59145548123", "Another user has changed the putaway job while you have been creating it. Please restart the operation and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);

						response.ReferencesOfReceiveThatCouldBeAutoFinalised = GetAllReceivesToAutoFinalise(receiveLines, parentTransferLines, Factory, staff);
					}
				}
			}
		}

		IEnumerable<WhsTransferLine> GetPutawayTransfersToFinalise(IEnumerable<(WhsInventoryView, WhsReceiveLine)> receiveLines, WhsPalletWebServiceResponse response, WhsLocation location, string putawayPalletId = null)
		{
			var result = new List<WhsTransferLine>();
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var whsPK = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK;
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>();
			var newCycleCountLocationPKs = new HashSet<ZGuid>();

			foreach (var (inventory, receiveLine) in receiveLines)
			{
				var lineToPutaway = (ILineToPutaway)receiveLine;
				var receive = receiveLine.Docket;
				var palletIDToUse = putawayPalletId ?? lineToPutaway.PalletID;
				var needDifferentLocationPutAwayEvent = !lineToPutaway.LocationPK.IsEmpty && !location.PK.Equals(lineToPutaway.LocationPK);
				var needDifferentPalletIdPutawayEvent = !lineToPutaway.PalletID.EqualsIgnoringCase(palletIDToUse);
				var oldLocation = inventory.Location;
				var oldLocationString = oldLocation?.WLV_LocationString ?? string.Empty;
				var oldAllocatedPalletId = lineToPutaway.PalletID;

				if ((needDifferentLocationPutAwayEvent || needDifferentPalletIdPutawayEvent) && receiveLine.IsFinalised)
				{
					response.LogBusinessValidationError(Res.GetString("ACBDF5FD-7E08-402C-B918-4B3A5056890B", "This pallet should be putaway into {0} Pallet ID {1}, as some inventory from this Pallet ID was already finalized into the location.", oldLocationString, oldAllocatedPalletId));
					return Enumerable.Empty<WhsTransferLine>();
				}
				else
				{
					lineToPutaway.LocationPK = location.PK;
					if (needDifferentPalletIdPutawayEvent)
					{
						lineToPutaway.PalletID = palletIDToUse;
					}

					PutawayHelper.SetReceiveArrivalDate(new[] { receive });

					if (needDifferentLocationPutAwayEvent || needDifferentPalletIdPutawayEvent)
					{
						var newLocation = location.ToLocationString();
						var oldLocationOnEvent = oldLocationString.IsEmpty ? location.ToLocationString() : oldLocationString;
						GenerateWarehouseReceiptConfirmedAlternateLocationEvent(receiveLine, oldLocationOnEvent, newLocation, oldAllocatedPalletId, palletIDToUse);
						CreateCycleCountForOldLocation(newCycleCountLocationPKs, cycleCountLocationCreator, inventory, receive, whsPK, oldLocation);
					}
					WebServiceHelper.GenerateWarehouseConfirmedPutAwayEvent(receiveLine);

					var putawayTransferLine = GetPutawayTransferLine(inventory, staff);
					if (putawayTransferLine != null)
					{
						result.Add(putawayTransferLine);
					}
				}
			}
			return result;
		}

		WhsTransferLine GetPutawayTransferLine(WhsInventoryView inventory, GlbStaff staff)
		{
			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			if (putawayTransferLine != null)
			{
				putawayTransferLine.WE_GS_NKPutawayBy = staff.GS_Code;
				putawayTransferLine.RunPreSaveValidationWithFetchHints();
			}

			return putawayTransferLine;
		}

		List<(WhsInventoryView Inventory, WhsReceiveLine ReceiveLine)> GetReceiveLinesForPutaway(IEnumerable<WhsInventoryView> inventoryCollection)
		{
			var result = new List<(WhsInventoryView, WhsReceiveLine)>();

			foreach (var inventory in inventoryCollection)
			{
				var receiveLine = inventory.InDocketLine as WhsReceiveLine;

				if (receiveLine == null
					&& inventory.InDocketLine is WhsTransferLine transferLine
					&& transferLine.IsPutawayTransferLine)
				{
					receiveLine = transferLine.AssociatedReceiveLineOfPutawayTransfer;
					if (receiveLine == null)
					{
						throw new InvalidOperationException($"Inventory being Putaway must have related Receive Line.");
					}
				}

				if (receiveLine != null)
				{
					result.Add((inventory, receiveLine));
				}
			}

			return result;
		}

		void FinalisePutawayJobAndLineAsRequired(WhsPalletWebServiceResponse response, string palletID, WhsWarehouse warehouse, GlbStaff staff)
		{
			var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
			if (putawayJob == null)
			{
				response.LogBusinessValidationError(
					Res.GetString("b26b08ee-90e9-49e5-9fc5-288dd18892ed", "Pallets putaway during a multiple pallet putaway must have a putaway job."));
			}
			else
			{
				var putawayLines = putawayJob.Lines;
				var putawayLine = putawayLines.SingleOrDefault(l => l.WPL_PalletID.EqualsIgnoringCase(palletID) && !l.WPL_IsFinalized);
				if (putawayLine == null)
				{
					response.LogBusinessValidationError(
						Res.GetString("89410d78-fa08-49db-b3c0-78bd5865e04e", "Pallets putaway during a multiple pallet putaway must have valid un-finalized putaway line."));
				}
				else
				{
					putawayLine.FinalizeLine();
					if (putawayLines.All(l => l.WPL_IsFinalized))
					{
						putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow;
					}
				}
			}
		}

		static List<string> GetAllReceivesToAutoFinalise(IEnumerable<WhsReceiveLine> receiveLines, WhsTransferLine[] putawayTransferLines, BusinessObjectFactory factory, GlbStaff staff)
		{
			var result = new List<string>();
			var receiveCanBeFinalisedDictionary = new Dictionary<ZGuid, bool>();

			foreach (var receiveLine in receiveLines)
			{
				var receive = receiveLine.Docket;
				if (receive != null && !receive.IsFinalised && !result.Contains(receive.WD_DocketID))
				{
					var canFinalise = ReceiveCanBeFinalised(receive, factory, receiveCanBeFinalisedDictionary, putawayTransferLines, staff) && !receive.Lines.Any(i => IsUnableToFinalise(i));
					if (canFinalise)
					{
						result.Add(receive.WD_DocketID);
					}
				}
			}

			return result;

			bool IsUnableToFinalise(WhsDocketLine inv) => WebServiceHelper.FindExistingLog(inv, ZArchitecture.Business.Events.WarehouseReceiptConfirmedPutaway, "RF") == null;
		}

		static bool ReceiveCanBeFinalised(WhsReceive receive, BusinessObjectFactory factory, Dictionary<ZGuid, bool> receiveCanBeFinalisedDictionary, WhsTransferLine[] putawayTransferLines, GlbStaff staff)
		{
			if (!receiveCanBeFinalisedDictionary.TryGetValue(receive.PK, out var canFinalise))
			{
				receiveCanBeFinalisedDictionary[receive.PK] = canFinalise = !receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned)
					|| (TaskManagementHelper.PlannedReceiveCanSetUnloadCompleteTime(receive, factory, staff) && HasNoActivePutawayTransferTasksAssignedToOtherUsers());
			}

			return canFinalise;

			bool HasNoActivePutawayTransferTasksAssignedToOtherUsers()
			{
				var receivePalletIds = receive.Lines.Select(l => l.WE_PalletID).ToHashSet();
				var putawayTransferPKs = putawayTransferLines
					.Where(line => receivePalletIds.Contains(line.WE_PalletID))
					.Select(line => line.WE_WD)
					.ToHashSet();

				var putawayJobTasksQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, putawayTransferPKs);
				putawayJobTasksQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PutawayJob);
				putawayJobTasksQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
				putawayJobTasksQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, SQLComparisonOperator.NotEqual, staff.GS_Code);
				return factory.LoadTop1<ProcessTask>(putawayJobTasksQuery) is null;
			}
		}

		static void GenerateWarehouseReceiptConfirmedAlternateLocationEvent(WhsDocketLine inventoryLine, string oldLocation, string newLocation, string oldPalletId, string newPalletId)
		{
			var logType = ZArchitecture.Business.Events.WarehouseReceiptConfAltLocn;

			var newLog = inventoryLine.Logs.AddNew(logType, ZString.Format("RF: Allocated Location for Line {0} and Pallet ID {1} was {2}. Putaway confirmed {3} and Pallet ID {4}.",
				inventoryLine.WE_LineNo, oldPalletId, oldLocation, newLocation, newPalletId));
			newLog.SL_GS_NKUser = Enterprise.Environment.Env.CurrentUser.Initials;
		}

		void CreateCycleCountForOldLocation(HashSet<ZGuid> existingCycleCountLocationPKs, IWhsCycleCountLocationCreator cycleCountLocationCreator, WhsInventoryView inventory, WhsReceive receive, ZGuid whsPK, WhsLocation oldLocation)
		{
			if (oldLocation != null && existingCycleCountLocationPKs.Add(oldLocation.PK))
			{
				var clientParams = WhsClientParams.GetClientParams(inventory.Client);
				var whsClientParameterByWarehouseCollection = clientParams.ClientParametersByWarehouse;
				var whsClientParameterByWarehouse = whsClientParameterByWarehouseCollection.FindWithEmptyFallback(inventory.Client.PK, whsPK, receive.WD_ReceiveCategory);
				if (whsClientParameterByWarehouse != null && whsClientParameterByWarehouse.WY_CycleCountOnAlternatePutaway)
				{
					cycleCountLocationCreator.CreateCycleCountLocation(oldLocation, priority: 1);
				}
			}
		}

		void ClosePutawayTaskIfAllLinesFinalised(
			WhsPalletWebServiceResponse response,
			WhsWarehouse warehouse,
			GlbStaff staff,
			IEnumerable<WhsTransferLine> putawayTransferLinesToFinalise,
			ZGuid taskPK)
		{
			if (warehouse.IsTaskManagementEnabled && taskPK.IsValid)
			{
				var query = new ZQuery(WhsDocketLineSchema.WE_P9_Task, taskPK);
				query.AddToFilter(WhsDocketLineSchema.PK, SQLComparisonOperator.NotEqual, putawayTransferLinesToFinalise.Select(l => l.PK));
				query.AddToFilter(WhsDocketLineSchema.WE_FinalisedDate, null);

				var noOtherLinesOnTask = !Factory.Exists(typeof(WhsTransferLine), query);
				if (noOtherLinesOnTask)
				{
					BeginRFTaskHelper.CloseRFTask(response, Factory.Load<WhsTransferProcessTasks>(taskPK), staff);
				}
			}
		}
	}
}
