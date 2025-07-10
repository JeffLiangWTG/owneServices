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
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetWhsPick

		[WebMethod(Description = "Get pick data")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickWebServiceResponse GetWhsPick(string reference, SearchFilterCriteriaInfo criteria)
		{
			return HandleWebServiceRequest<WhsPickWebServiceResponse>(r => GetWhsPick(r, reference, criteria));
		}

		void GetWhsPick(WhsPickWebServiceResponse response, string reference, SearchFilterCriteriaInfo criteria)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (staff != null && warehouse != null)
			{
				var findPickResult = LoadWhsPick(response, reference, staff, warehouse, criteria);
				if (response.NoError())
				{
					PrepareTaskForPick(reference, response, staff, findPickResult, warehouse);
					SetPickWebServiceResponse(reference, response, findPickResult);
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service."));
			}
		}

		FindPickResult LoadWhsPick(WhsPickWebServiceResponse response, string reference, GlbStaff staff, WhsWarehouse warehouse, SearchFilterCriteriaInfo criteria)
		{
			FindPickResult result;
			if (warehouse.IsTaskManagementEnabled && string.IsNullOrEmpty(reference))
			{
				result = LoadWhsPickForTaskManagement(response, staff, warehouse);
			}
			else
			{
				const int RetryGetPickAmount = 2;
				var retries = 0;
				var linesPicker = new PickLinesLoader(warehouse.Factory, warehouse, staff, criteria);
				result = linesPicker.FindAndAssignNextPick(reference);

				while (result.IsConcurrencyError && retries < RetryGetPickAmount)
				{
					result = linesPicker.FindAndAssignNextPick(reference);
					retries++;
				}
			}
			return result;
		}

		FindPickResult LoadWhsPickForTaskManagement(WhsPickWebServiceResponse response, GlbStaff staff, WhsWarehouse warehouse)
		{
			FindPickResult result;
			var taskManagementService = ObjectFactory.Get<IWhsTaskManagementService>();
			var getNextTaskResult = taskManagementService.GetNextTaskForWarehouseWeb(
				Factory,
				staff,
				warehouse.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				Array.Empty<Guid>());

			if (string.IsNullOrEmpty(getNextTaskResult.ErrorMessage))
			{
				var unpickedLinesQuery = new ZQuery(WhsPickLineSchema.WZ_P9_Task, getNextTaskResult.TaskPK);
				unpickedLinesQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);
				unpickedLinesQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, null);
				var unpickedLines = Factory.Load<WhsPickLine>(unpickedLinesQuery);

				var task = Factory.Load<WhsPickProcessTask>(getNextTaskResult.TaskPK);
				var pick = task.Parent;

				result = unpickedLines.Any()
					? new FindPickResult(pick, unpickedLines, task)
					: new FindPickResult(pick, task);
			}
			else
			{
				result = null;
				response.LogBusinessValidationError(getNextTaskResult.ErrorMessage);
			}

			return result;
		}

		void SetPickWebServiceResponse(string reference, WhsPickWebServiceResponse response, FindPickResult findPickResult)
		{
			WhsPickInfo pick;
			if (findPickResult.Pick != null)
			{
				pick = findPickResult.IsPutawayOnlyPickResult
					? new WhsPickInfo(findPickResult.Pick)
					: new WhsPickInfo(findPickResult.Pick, findPickResult.PickLines, GetCompletePallets(findPickResult.Pick));
			}
			else
			{
				pick = new WhsPickInfo();
			}

			if (pick.Lines.Count == 0 && !findPickResult.IsPutawayOnlyPickResult)
			{
				var message = findPickResult.ErrorMessage.IsEmpty
					? Res.GetString("02a4af41-852c-4544-9ab3-5c8416421d75", "Un-finalized pick could not be found {0}. Possible mismatch on registered equipment, registered pick group, registered area, registered client, pick has been assigned to another operator or customs order is on hold.",
						string.IsNullOrEmpty(reference) ? "" : Res.GetString("f2d98d2c-572e-4c73-9492-0f610b6d89b8", "for reference: {0}", reference))
					: findPickResult.ErrorMessage.ToString();

				response.LogBusinessValidationError(message);
			}
			else
			{
				response.Pick = pick;
				AddWhsEventLogCore(response, AutoEvents.ServiceCommencedCode, WhsPickSchema.Constants.Prefix, pick.PK);
			}
		}

		void PrepareTaskForPick(string reference, WhsPickWebServiceResponse response, GlbStaff staff, FindPickResult findPickResult, WhsWarehouse warehouse)
		{
			if (findPickResult.Pick != null)
			{
				var pickTask = findPickResult.Task ?? ResolveTaskForSelectedPickJob(staff);
				if (pickTask != null)
				{
					BeginRFTaskHelper.BeginRFTask(response, pickTask, WarehouseTaskFormFlowTypes.PickJob, staff);
					response.TaskPK = pickTask.PK.ToGuid();
				}
			}

			WhsPickProcessTask ResolveTaskForSelectedPickJob(GlbStaff staff)
			{
				WhsPickProcessTask task = null;
				if (warehouse.IsTaskManagementEnabled)
				{
					var linesToPutaway = findPickResult.Pick
						.Transfers
						.SelectMany(t => t.Lines)
						.Where(l => l.WE_PutawayTime.IsEmpty)
						.SelectMany(l => l.PickLines);

					var allPickLines = new List<WhsPickLine>();
					var allTaskPKs = new HashSet<ZGuid>();
					foreach (var pickLine in findPickResult.PickLines.Union(linesToPutaway))
					{
						allTaskPKs.Add(pickLine.WZ_P9_Task);
						allPickLines.Add(pickLine);
					}

					var firstTaskPK = allTaskPKs.Count == 1 ? allTaskPKs.First() : ZGuid.Empty;
					if (firstTaskPK.IsValid)
					{
						var allPickLinesForTask = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, firstTaskPK));
						if (DoPickLineCollectionsMatch(allPickLines, allPickLinesForTask))
						{
							task = Factory.Load<WhsPickProcessTask>(firstTaskPK);
						}
					}

					if (task == null)
					{
						var originalTaskPKs = allTaskPKs.ToArray();

						task = CreatePickTask(findPickResult.Pick, staff);
						allPickLines.ForEach(pl => pl.WZ_P9_Task = task.PK);

						HandleOrphanedTasks(originalTaskPKs, staff);
					}
				}

				return task;
			}

			bool DoPickLineCollectionsMatch(List<WhsPickLine> firstCollection, WhsPickLine[] secondCollection)
			{
				var result = firstCollection.Count == secondCollection.Length;
				if (result)
				{
					firstCollection.Sort(SortByPK);
					Array.Sort(secondCollection, SortByPK);

					result = firstCollection.SequenceEqual(secondCollection);
				}

				return result;

				int SortByPK(WhsPickLine line1, WhsPickLine line2) => line1.PK.CompareTo(line2.PK);
			}

			void HandleOrphanedTasks(IEnumerable<ZGuid> taskPKsToCheck, GlbStaff staff)
			{
				taskPKsToCheck.ForEach(taskPK => Factory.AddFetchHint(StmALogSchema.SL_Parent, taskPK));
				var workingTaskQuery = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code);
				workingTaskQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
				Factory.AddFetchHint(ProcessTasksSchema.Instance, workingTaskQuery);

				var tasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, taskPKsToCheck)).ToDictionary(t => t.PK);
				var remainingTaskPickLines = Factory
					.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, taskPKsToCheck))
					.GroupBy(pl => pl.WZ_P9_Task);

				foreach (var groupedPickLines in remainingTaskPickLines)
				{
					var task = tasks[groupedPickLines.Key];
					if (groupedPickLines.All(IsPickLinePickedAndPutaway))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}

					tasks.Remove(groupedPickLines.Key);
				}

				ProcessTaskHelper.DeleteProcessTasksAndRelatedProcessHeader(Factory, tasks.Values.ToArray());

				// Any picked line that is not putaway has been reassigned to the new task
				// So any remaining picked line MUST be putaway
				bool IsPickLinePickedAndPutaway(WhsPickLine pickLine)
					=> pickLine.WZ_PickedDateTime.IsValid || pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid;
			}

			WhsPickProcessTask CreatePickTask(WhsPick pick, GlbStaff staff)
			{
				var pickTask = pick.Factory.New<WhsPickProcessTask>();
				pickTask.P9_ParentID = pick.PK;
				pickTask.P9_ParentTableCode = WhsPickSchema.Constants.Prefix;
				pickTask.P9_FormFlowType = WarehouseTaskFormFlowTypes.PickJob;
				SetDefaultTaskDetails(pickTask, $"Temporary Picking Task Description", staff.GS_Code, isWorking: false);

				return pickTask;
			}
		}

		List<string> GetCompletePallets(WhsPick pick)
		{
			var result = new List<string>();

			var relatedInventory = pick.GetAllPickLines().DistinctBy(pl => pl.WZ_WE_InventoryLine).Select(pl => pl.Inventory);
			var palletIDsInPickLines = relatedInventory.Select(i => i.WI_PalletID).Where(pid => !pid.IsEmpty).Distinct().ToArray();

			if (palletIDsInPickLines.Length > 0)
			{
				var sql = @"
SELECT
	PickLine.WI_PalletID
FROM
	(
		SELECT
			SUM(WhsPickLine.WZ_Units) AS Units,
			Inventory.WE_PalletID AS WI_PalletID
		FROM
			dbo.WhsPickLine
			JOIN dbo.WhsDocketLine TransactionLine ON TransactionLine.WE_PK = WZ_WE_TransactionLine
			JOIN dbo.WhsDocket ON WD_PK = TransactionLine.WE_WD
			JOIN dbo.WhsDocketLine Inventory ON WZ_WE_InventoryLine = Inventory.WE_PK
		WHERE
			WD_WP = @PickPK AND
			Inventory.WE_PalletID != ''
		GROUP BY
			Inventory.WE_PalletID
	) AS PickLine
	JOIN
	(
		SELECT	
			SUM(WI_TotalUnits) AS Units,
			WI_PalletID
		FROM
			dbo.WhsInventoryView
			JOIN dbo.WhsDocket ON WI_WD = WD_PK
		WHERE
			WI_WW_WHS = @WhsPK AND
			WD_WP_ParentPickForTransfer IS NULL AND -- ignore stock created for dock door movements
			WI_TotalUnits > 0 AND
			WI_PalletID IN (SELECT value FROM @PalletIDs) AND
			WI_PalletID <> ''
		GROUP BY
			WhsInventoryView.WI_PalletID
	) AS Inventory ON PickLine.WI_PalletID = Inventory.WI_PalletID
WHERE
	PickLine.Units = Inventory.Units";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@PickPK", pick.PK, WhsDocketSchema.WD_WP);
				sqlParams.Add("@WhsPK", pick.WP_WW_Whs, WhsInventoryViewSchema.WI_WW_Whs);
				sqlParams.Add(ZSqlParameter.New("@PalletIDs", palletIDsInPickLines, WhsInventoryViewSchema.WI_PalletID, true));

				var palletIDCollection = new DynamicBusinessObjectCollection(Factory);
				palletIDCollection.Load(sql, sqlParams);

				result.AddRange(palletIDCollection.Select(p => ((ZString)p[WhsInventoryViewSchema.Constants.WI_PalletID]).ToString()));
			}

			return result;
		}

		#endregion
	}
}
