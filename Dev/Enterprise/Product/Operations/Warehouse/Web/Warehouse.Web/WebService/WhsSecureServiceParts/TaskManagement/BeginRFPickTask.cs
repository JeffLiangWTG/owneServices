using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFPickTask

		[WebMethod(Description = "Begin RF Pick Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickWebServiceResponse BeginRFPickTask(Guid taskPK)
		{
			return HandleWebServiceRequest((WhsPickWebServiceResponse r) => BeginRFPickTaskCore(r, taskPK));
		}

		void BeginRFPickTaskCore(WhsPickWebServiceResponse response, Guid taskPK)
		{
			var pickTaskQuery = new ZQuery(ProcessTasksSchema.PK, taskPK);
			pickTaskQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PickJob);
			var task = Factory.LoadTop1<WhsPickProcessTask>(pickTaskQuery);

			if (task != null)
			{
				var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				PrepareTaskForPick(response, task, staff);
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("aeb3ec91-b2a9-42d0-a152-69a0a9c76cd1", "Picking Task could not be found."));
			}
		}

		void PrepareTaskForPick(WhsPickWebServiceResponse response, WhsPickProcessTask task, GlbStaff staff)
		{
			var pick = Factory.Load<WhsPick>(task.P9_ParentID);

			var pickLinePKs = LoadPickLinePKs(task);
			var pickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLinePKs));

			if (pickLines.Length > 0)
			{
				AddFetchHintsForPickLines(pickLines);

				SetAssignedUserOnRelatedJobs(pickLines, staff.GS_Code);

				PrepareResponseWithPickDetails(response, pick, pickLines);

				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => BeginRFTaskHelper.TaskConcurrencyErrorMessage);
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("68bb400e-4ad6-4a8a-a02a-aba190bc92f8", "No Pick Lines could be found for this task for current user."));
			}

			ZGuid[] LoadPickLinePKs(ProcessTask task)
			{
				var query = @"SELECT WZ_PK
FROM
	WhsPickLine
	LEFT JOIN dbo.WhsDocketLine AS PutawayLine ON PutawayLine.WE_PK = WZ_WE_InventoryLine AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL
WHERE
	WZ_P9_Task = @TaskPK
	AND (WZ_WE_OriginalPickedInventoryLine IS NOT NULL OR WZ_PickedDateTime IS NULL) -- Is Not Picked
	AND (WZ_WE_OriginalPickedInventoryLine IS NULL OR PutawayLine.WE_PutawayTime IS NULL) -- Is Not Putaway";

				var results = new DynamicBusinessObjectCollection(Factory);
				var parameters = new ZSqlParameterCollection();
				parameters.Add("@TaskPK", task.PK, WhsPickLineSchema.WZ_P9_Task);
				results.Load(query, parameters);
				return results.Select(r => (ZGuid)r["WZ_PK"]).ToArray();
			}

			void SetAssignedUserOnRelatedJobs(WhsPickLine[] pickLines, string staffCode)
			{
				foreach (var pickline in pickLines)
				{
					pickline.WZ_GS_NKAssignedTo = staffCode;
				}
			}

			void PrepareResponseWithPickDetails(WhsPickWebServiceResponse response, WhsPick pick, WhsPickLine[] pickLines)
			{
				FindPickResult pickResult;

				var linesToPick = pickLines.Where(l => l.WZ_WE_OriginalPickedInventoryLine.IsEmpty).ToArray();
				if (linesToPick.Length > 0)
				{
					Array.Sort(linesToPick, new SortPickLinesForPickingSlip());

					pick.UpdateIsLocationEmptyAfterFinalisingPickOnPickLines(linesToPick);
					foreach (var line in linesToPick)
					{
						line.WZ_IsPicking = true;
					}

					pickResult = new FindPickResult(pick, linesToPick, task);
				}
				else
				{
					// Putaway Only Pick
					pickResult = new FindPickResult(pick, task);
				}

				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				PrepareTaskForPick(pick.WP_PickNo, response, staff,pickResult, warehouse);
				SetPickWebServiceResponse(pick.WP_PickNo, response, pickResult);
			}

			void AddFetchHintsForPickLines(WhsPickLine[] pickLines)
			{
				pickLines.ForEach(pl =>
				{
					Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, pl.WZ_WE_InventoryLine);
					Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, pl.WZ_WE_TransactionLine);
				});
			}
		}

		#endregion
	}
}
