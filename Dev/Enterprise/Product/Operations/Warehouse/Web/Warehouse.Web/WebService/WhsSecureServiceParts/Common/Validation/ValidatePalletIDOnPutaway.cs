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
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region ValidatePalletIDOnPutaway

		[WebMethod(Description = "Validate Pallet ID On Putaway")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPalletWebServiceResponse ValidatePalletIDOnPutaway(string palletID, bool isReassigning)
		{
			return HandleWebServiceRequest<WhsPalletWebServiceResponse>(response =>
			{
				var inventoryCollection = WebServiceHelper.LoadWhsInventoryByPalletID(response, Factory, SecurityHeader.WarehouseCode, palletID);
				if (response.NoError())
				{
					response.PalletID = palletID;
					response.AllowToOverrideLocation = AllowToOverrideLocation(inventoryCollection);
					response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;

					ValidatePalletIDOnPutawayCore(response, isReassigning, palletID);
				}
			});
		}

		bool AllowToOverrideLocation(IEnumerable<WhsInventoryView> inventoryCollection)
		{
			AddInventoryDocketFetchHints(inventoryCollection, Factory);
			foreach (var inventory in inventoryCollection)
			{
				var docket = inventory.Docket;
				if (docket != null && docket.IsFinalised)
				{
					return false;
				}
			}

			return true;
		}

		void ValidatePalletIDOnPutawayCore(WhsPalletWebServiceResponse response, bool isReassigning, string palletID)
		{
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var inventories = Factory.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery(new string[] { palletID }, whs.PK));
			if (inventories.Length == 0)
			{
				response.LogBusinessValidationError(Res.GetString("658c020b-ba31-4898-8232-6548c4b9dfc5", "Pallet ID {0} does not exist on an Un-Finalized Receipt.", palletID));
			}
			else if (inventories.Any(i => i.Docket.WD_DocketSubType == ReceiveType.Codes.Customs && !i.IsReceivedIntoDockDoor))
			{
				response.LogBusinessValidationError(Res.GetString("842ac30e-f43d-40d1-b931-6c5861419c61", "Bonded Pallet should be unloaded on the desktop."));
			}

			PutawayHelper.CheckIfPalletIDsIsHeldForPutaway(response, inventories);

			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var isReassigningToSelf = CheckIfPalletIDAssignedToOwnPutawayJob(response, palletID, isReassigning, whs, staff);
			if (response.NoError())
			{
				var inventoriesToPutaway = inventories.Where(inventory => inventory.WI_InDocketLineUnits > 0).ToArray();
				if (inventoriesToPutaway.Length > 0)
				{
					var helper = ObjectFactory.Get<ICreatePutawayTransferHelper>();
					var errorMessage = helper.CreateTransfer(Factory, whs, inventories, false, staff.GS_Code);
					if (!errorMessage.IsNullOrEmpty())
					{
						response.LogError(ErrorTypes.BusinessValidationError, errorMessage);
					}

					HandlePutawayTask(response, whs, staff, inventoriesToPutaway, palletID);
					UpdatePutawayJobWithPalletIDAndPopulatePalletInfo(response, isReassigning, palletID, inventoriesToPutaway, isReassigningToSelf, whs);
					response.Inventory = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(inventories);

					var client = inventoriesToPutaway[0].Client;
					response.PalletInfo.ConsolidateProductInfos(
						response.Inventory.ProductInfos.Select(pi => pi.Code),
						client.PartAttributeManager.PartAttributeName1,
						response.Inventory.InventoryLineInfos.Select(inv => inv.Attribute1),
						client.PartAttributeManager.PartAttributeName2,
						response.Inventory.InventoryLineInfos.Select(inv => inv.Attribute2),
						client.PartAttributeManager.PartAttributeName3,
						response.Inventory.InventoryLineInfos.Select(inv => inv.Attribute3),
						response.Inventory.InventoryLineInfos.Select(inv => inv.SerialNumber),
						response.Inventory.InventoryLineInfos.Select(inv => inv.ExpiryDate),
						response.Inventory.InventoryLineInfos.Select(inv => inv.PackingDate));
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("D296569B-D267-4883-9DC1-EC35CDB0CFD3", "There are no items to putaway."));
				}
			}
		}

		bool CheckIfPalletIDAssignedToOwnPutawayJob(WhsPalletWebServiceResponse response, string palletID, bool isReassigning, WhsWarehouse warehouse, GlbStaff staff)
		{
			var isReassigningToSelf = false;
			if (response.NoError())
			{
				var lines = PutawayLinesOnPutawayJobWithSameIDAsScannedPalletID(warehouse, palletID);
				if (lines.Any())
				{
					var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
					if (putawayJob != null && lines.Any(l => l.WPL_WPJ_PutawayJob == putawayJob.PK))
					{
						isReassigningToSelf = true;
					}
					else
					{
						PutawayHelper.CheckIfPalletIDsNotOnAnotherPutawayJob(response, lines, isReassigning);
					}
				}
			}
			return isReassigningToSelf;
		}

		void UpdatePutawayJobWithPalletIDAndPopulatePalletInfo(
			WhsPalletWebServiceResponse response,
			bool isReassigning,
			string palletID,
			WhsInventoryView[] inventoriesToPutaway,
			bool isReassigningToSelf,
			WhsWarehouse warehouse)
		{
			if (response.NoError())
			{
				if (isReassigning && !isReassigningToSelf)
				{
					var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
					UpdateAssociatedPutawayTransferLinesPutawayBy(inventoriesToPutaway, staff);
				}

				DeleteNotPuttingAwayLineForPalletID(warehouse, palletID);

				var concurrencyErrorMessage = Res.GetString("785c9275-97f6-45d9-bfb1-4bf6758f9378", "Another user has changed the putaway job while you have been working on it. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		void DeleteNotPuttingAwayLineForPalletID(WhsWarehouse warehouse, string palletID)
		{
			var putawayLines = PutawayLinesOnPutawayJobWithSameIDAsScannedPalletID(warehouse, palletID);
			PutawayHelper.DeleteLinesWhichAreNotPuttingAway(putawayLines);
		}

		void HandlePutawayTask(WhsPalletWebServiceResponse response, WhsWarehouse warehouse, GlbStaff staff, WhsInventoryView[] inventoriesToPutaway, string palletID)
		{
			if (response.NoError() && warehouse.IsTaskManagementEnabled)
			{
				var putawayTransferLines = inventoriesToPutaway.Select(PutawayHelper.GetPutawayTransferLineFromInventory).WhereNotNull().DistinctBy(l => l.PK).ToArray();
				var taskPKs = putawayTransferLines.Select(tl => tl.WE_P9_Task).ToHashSet();
				var existingPutawayTasks = Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.PK, taskPKs));
				if (existingPutawayTasks.Any(t => t.P9_GS_NKAssignedStaffMember != staff.GS_Code && t.P9_Status == ProcessTaskStatusCodeList.Codes.Working))
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("1952e9a4-50fe-46cc-9761-2aa653706837", "Scanned Pallet ID has a working task assigned to another user."));
				}
				else
				{
					var putawayTask = existingPutawayTasks.FirstOrDefault(t => (t.P9_GS_NKAssignedStaffMember == staff.GS_Code && t.P9_Status != ProcessTaskStatusCodeList.Codes.Closed) || t.P9_Status == ProcessTaskStatusCodeList.Codes.Open);
					if (putawayTask == null || OtherPalletsExistOnTask(putawayTask, palletID))
					{
						putawayTask = CreatePutawayTask(warehouse.Factory, putawayTransferLines[0].WE_WD, staff);
					}
					putawayTransferLines.ForEach(t => t.WE_P9_Task = putawayTask.PK);
					response.TaskPK = putawayTask.PK.ToGuid();

					if (putawayTask.P9_Status != ProcessTaskStatusCodeList.Codes.Working)
					{
						BeginRFTaskHelper.BeginRFTask(response, putawayTask, WarehouseTaskFormFlowTypes.PutawayJob, staff);
					}
					HandleOrphanedTasksForPutaway(existingPutawayTasks);
				}

				static WhsTransferProcessTasks CreatePutawayTask(BusinessObjectFactory factory, ZGuid transferPK, GlbStaff staff)
				{
					var putawayTask = factory.New<WhsTransferProcessTasks>();
					putawayTask.P9_ParentID = transferPK;
					putawayTask.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
					putawayTask.P9_FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob;
					SetDefaultTaskDetails(putawayTask, $"Temporary Putaway Task Description", staff.GS_Code, isWorking: true);
					return putawayTask;
				}
			}
		}

		bool OtherPalletsExistOnTask(ProcessTask task, string palletID)
		{
			var query = new ZQuery(WhsDocketLineSchema.WE_P9_Task, task.PK);
			query.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, palletID);
			return Factory.Exists(typeof(WhsTransferLine), query);
		}

		static void HandleOrphanedTasksForPutaway(ProcessTask[] tasks)
		{
			if (tasks.Length > 0)
			{
				var taskDictionary = tasks.ToDictionary(t => t.PK);
				var transferLineQuery = new ZQuery(WhsDocketLineSchema.WE_P9_Task, taskDictionary.Keys);
				var transferLinesOnTasks = tasks[0].Factory.Load<WhsTransferLine>(transferLineQuery).GroupBy(pl => pl.WE_P9_Task);

				foreach (var groupedLines in transferLinesOnTasks)
				{
					var task = taskDictionary[groupedLines.Key];
					if (groupedLines.All(l => l.IsFinalised))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}

					taskDictionary.Remove(groupedLines.Key);
				}
				taskDictionary.Values.ForEach(t => t.Delete());
			}
		}

		#region PutawayLinesOnPutawayJobsWithSameIDAsScannedPalletID

		IEnumerable<WhsPutawayLine> putawayLineOnPutawayJobWithSameID;
		IEnumerable<WhsPutawayLine> PutawayLinesOnPutawayJobWithSameIDAsScannedPalletID(WhsWarehouse warehouse, string palletID)
			=> putawayLineOnPutawayJobWithSameID ?? (putawayLineOnPutawayJobWithSameID = PutawayHelper.LoadPutawayLinesWithMatchingPalletIDs(Factory, warehouse, new[] { palletID }));

		#endregion

		#region AddFetchHints

		static void AddInventoryDocketFetchHints(IEnumerable<WhsInventoryView> inventoryCollection, BusinessObjectFactory factory)
		{
			foreach (var inventory in inventoryCollection)
			{
				factory.AddFetchHint(WhsDocketSchema.Constants.TableName, inventory.WI_WD_Proxy);
			}
		}

		void AddPutawayTransferFetchHints(IEnumerable<WhsDocketLine> receiveLines, BusinessObjectFactory factory)
		{
			var inventoriesQuery = new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, receiveLines.Select(receiveLine => receiveLine.PK));
			factory.AddFetchHint(typeof(WhsInventoryView), inventoriesQuery);

			var docketsQuery = new ZQuery(WhsDocketSchema.PK, receiveLines.Select(receiveLine => receiveLine.WE_WD));
			factory.AddFetchHint(typeof(WhsReceive), docketsQuery);

			var allInventories = receiveLines.SelectMany(line => line.Inventory).Cast<WhsInventoryView>();
			AddInventoryToPickLineFetchHints(allInventories, factory);

			var allPickLines = allInventories.SelectMany(inv => inv.AllPickLines);
			foreach (var pickLine in allPickLines)
			{
				factory.AddFetchHint(typeof(WhsDocketLine), pickLine.WZ_WE_TransactionLine);
			}
			LoadAllPutawayTransfers(allPickLines, factory);
		}

		void AddInventoryToPickLineFetchHints(IEnumerable<WhsInventoryView> inventories, BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventories.Select(inventory => inventory.WI_WE_InDocketLine));
			factory.AddFetchHint(typeof(WhsPickLine), query);
		}

		IEnumerable<WhsDocket> LoadAllPutawayTransfers(IEnumerable<WhsPickLine> allPickLines, BusinessObjectFactory factory)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.PK, allPickLines.Select(l => l.PK));

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var putawayTransferQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			putawayTransferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);
			putawayTransferQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);

			return factory.Load<WhsDocket>(putawayTransferQuery);
		}

		#endregion

		#endregion
	}
}
