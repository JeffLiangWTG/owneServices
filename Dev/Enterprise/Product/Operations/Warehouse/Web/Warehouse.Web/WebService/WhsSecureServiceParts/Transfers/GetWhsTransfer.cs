using System;
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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region GetWhsTransfer

		[WebMethod(Description = "Get transfer data for reference")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse GetWhsTransfer(string reference, SearchFilterCriteriaInfo criteria, bool isForcedPutaway, Guid[] exceptedPKs)
		{
			return HandleWebServiceRequest((WhsDocketWebServiceResponse r) => GetWhsTransferCore(r, reference, criteria, isForcedPutaway, exceptedPKs));
		}

		WhsDocketWebServiceResponse GetWhsTransferCore(WhsDocketWebServiceResponse response, string reference, SearchFilterCriteriaInfo criteria, bool isForcedPutaway, Guid[] exceptedPKs)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var clientPK = WhsTransferHelper.FindClientPK(Factory, criteria.ClientCode);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var (transfer, transferTask) = GetTransferAndAssociatedTask(
				response,
				reference,
				staff,
				warehouse,
				clientPK,
				criteria,
				exceptedPKs,
				WarehouseTaskFormFlowTypes.TransferJob,
				NoGetNextTransferAvailableErrorMessage,
				isReplenishmentTask: false,
				(transferReference) => GetWhsTransferWithReference(response, clientPK, transferReference, staff, warehouse, criteria)); // If no transfer is found or there are no assignable lines on the transfer we inform the user of this.

			if (response.NoError())
			{
				if (transferTask != null)
				{
					BeginRFTaskHelper.BeginRFTask(response, transferTask, WarehouseTaskFormFlowTypes.TransferJob, staff);
				}

				if (response.NoError())
				{
					var palletIdText = Res.GetString("51cccd44-2f07-4c36-9ace-cfe138e271e7", "/ Pallet ID ");
					var errorMessage = string.IsNullOrEmpty(reference)
						? warehouse.IsTaskManagementEnabled
							? NoGetNextTransferAvailableErrorMessage
							: Res.GetString("9fe19198-b749-4331-9ad1-5a5f31da5677", "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.")
						: Res.GetString("2c7fc905-361b-42ee-a16f-c471e256eac4", "Un-finalized transfer could not be found {0}. Possible mismatch on registered equipment, registered area, registered client or transfer {1} has been assigned to another operator",
						Res.GetString("5da52e35-88f6-43ae-9025-8536aa9413d9", "for Reference {0}: {1}", transfer != null ? palletIdText : "", reference), palletIdText);

					PrepareTransferLinesAndSave(
						Factory,
						response,
						criteria,
						staff,
						transfer,
						transferTask,
						errorMessage,
						concurrencyErrorMessage: WhsTransferHelper.WhsTransferConcurrencyError,
						isForcedPutaway: isForcedPutaway,
						canShowStockOnHandWarningOnPutaway: true);
				}
			}

			return response;
		}

		WhsTransfer GetWhsTransferWithReference(WebServiceResponse response, ZGuid clientPK, string reference, GlbStaff staff, WhsWarehouse warehouse, SearchFilterCriteriaInfo criteria)
		{
			WhsTransfer transfer = null;
			if (clientPK.IsEmpty || clientPK.IsValid)
			{
				var additionalFilter = new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, null);
				additionalFilter.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, 0);
				var clientPKToGuid = clientPK.IsValid ? clientPK.ToGuid() : (Guid?)null;
				transfer = WebServiceHelper.LoadWhsDocket<WhsTransfer>(Factory, reference, Res.GetString("29022982-0055-40fa-832b-fc176334a665", "Transfer"), DocketType.Codes.Transfer, SecurityHeader.WarehouseCode, additionalFilter, clientPKToGuid);
			}

			if (transfer == null)
			{
				transfer = LoadDocketFromPalletID(reference, staff, warehouse);
				if (transfer == null)
				{
					response.LogBusinessValidationError(Res.GetString("1855a024-29ad-4f20-8931-7e12f7ea9459", "Can't find un finalized Transfer with Reference: {0}.", reference));
				}
			}

			return transfer;
		}

		#region LoadDocketFromPalletID

		WhsTransfer LoadDocketFromPalletID(string reference, GlbStaff staff, WhsWarehouse warehouse)
		{
			var query = GetDocketFromPalletIDQuery(reference, staff, warehouse);
			var transfer = Factory.LoadTop1<WhsTransfer>(query);

			return transfer;
		}

		ZDBOnlyQuery GetDocketFromPalletIDQuery(string palletId, GlbStaff staff, WhsWarehouse warehouse)
		{
			var docketQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, palletId);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_GS_NKAssignedTo, SQLComparisonOperator.Equal, "");

			if (staff != null)
			{
				pickLineSubQuery.AddToFilter(JoinCondition.Or, WhsPickLineSchema.WZ_GS_NKAssignedTo, SQLComparisonOperator.Equal, staff.GS_Code);
			}

			docketLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);
			docketQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			docketQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);
			docketQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			docketQuery.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForTransfer, null);
			docketQuery.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, 0);

			return docketQuery;
		}

		#endregion

		#region GetTransferAndAssociatedTask

		(WhsTransfer, ProcessTask) GetTransferAndAssociatedTask(
			WebServiceResponse response,
			string reference,
			GlbStaff staff,
			WhsWarehouse warehouse,
			ZGuid clientPK,
			SearchFilterCriteriaInfo criteria,
			Guid[] exceptedPKs,
			string formFlowType,
			string noNextAvailableTransferErrorMessage,
			bool isReplenishmentTask,
			Func<string, WhsTransfer> getTransferWithReference)
		{
			WhsTransfer transfer = null;
			ProcessTask transferTask = null;

			if (warehouse != null)
			{
				(transfer, transferTask) = !string.IsNullOrEmpty(reference)
					? GetTransferAndAssociatedTaskWithTransferReference(response, reference, staff, warehouse, formFlowType, (reference) => getTransferWithReference(reference))
					: GetTransferAndAssociatedTaskWithEmptyTransferReference(Factory, response, clientPK, staff, warehouse, criteria, exceptedPKs, noNextAvailableTransferErrorMessage, formFlowType, isReplenishmentTask);
			}

			return (transfer, transferTask);
		}

		#region GetTransferAndAssociatedTaskWithTransferReference

		(WhsTransfer, ProcessTask) GetTransferAndAssociatedTaskWithTransferReference(
			WebServiceResponse response,
			string reference,
			GlbStaff staff,
			WhsWarehouse warehouse,
			string formFlowType,
			Func<string, WhsTransfer> getTransferWithReference)
		{
			var transfer = getTransferWithReference(reference);
			ProcessTask transferTask = null;
			if (response.NoError())
			{
				transferTask = GetTransferProcessTask(response, Factory, transfer, formFlowType, warehouse, staff);
			}

			return (transfer, transferTask);
		}

		static ProcessTask GetTransferProcessTask(WebServiceResponse response, BusinessObjectFactory factory, WhsTransfer transfer, string formFlowType, WhsWarehouse warehouse, GlbStaff staff)
		{
			if (!warehouse.IsTaskManagementEnabled)
			{
				return null;
			}

			var transferProcessTasks = factory.Load<ProcessTask>(TaskManagementHelper.GetJobTasksQuery(transfer, formFlowType));
			return transferProcessTasks.Length > 0
				? SelectProcessTaskToRun(response, staff, transferProcessTasks)
				: CreateTransferProcessTask(factory, transfer, formFlowType, staff);
		}

		static ProcessTask SelectProcessTaskToRun(WebServiceResponse response, GlbStaff staff, ProcessTask[] processTasks)
		{
			ProcessTask transferTask = null;
			if (processTasks.All(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed))
			{
				response.LogBusinessValidationError(Res.GetString("22af958f-3012-4964-859e-656a0a5c9ec1", "This transfer is already completed."));
			}
			else
			{
				var assignableProcessTasks = processTasks.Where(task => IsProcessTaskAssignable(task, staff.GS_Code)).ToArray();
				if (assignableProcessTasks.Length == 0)
				{
					response.LogBusinessValidationError(Res.GetString("bb53af0e-1d7b-4b71-ae7c-a45a62962b50", "This transfer is assigned to another user."));
				}
				else
				{
					transferTask = assignableProcessTasks.OrderByDescending(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
						.ThenByDescending(task => task.P9_GS_NKAssignedStaffMember == staff.GS_Code)
						.ThenBy(task => task.P9_SystemCreateTimeUtc)
						.First();
				}
			}

			return transferTask;
		}

		static bool IsProcessTaskAssignable(ProcessTask processTask, string staffCode)
		{
			return processTask.P9_Status != ProcessTaskStatusCodeList.Codes.Closed
				&& (processTask.P9_Status == ProcessTaskStatusCodeList.Codes.Open || processTask.P9_GS_NKAssignedStaffMember == staffCode);
		}

		static ProcessTask CreateTransferProcessTask(BusinessObjectFactory factory, WhsTransfer transfer, string formFlowType, GlbStaff staff)
		{
			var transferTask = factory.New<WhsTransferProcessTasks>();
			transferTask.P9_ParentID = transfer.PK;
			transferTask.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			transferTask.P9_FormFlowType = formFlowType;
			transferTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			// Temporary values to be replaced in a later WI.
			transferTask.P9_Description = (NoResString)"Temporary Transfer Task Description";
			transferTask.P9_Type = "UDF";

			transfer.Lines
				.Where(line => line.WE_P9_Task.IsEmpty)
				.ForEach(line => line.WE_P9_Task = transferTask.PK);

			return transferTask;
		}

		#endregion

		#region GetTransferAndAssociatedTaskWithEmptyTransferReference

		static (WhsTransfer, ProcessTask) GetTransferAndAssociatedTaskWithEmptyTransferReference(
			BusinessObjectFactory factory,
			WebServiceResponse response,
			ZGuid clientPK,
			GlbStaff staff,
			WhsWarehouse warehouse,
			SearchFilterCriteriaInfo criteria,
			Guid[] exceptedPKs,
			string errorMessage,
			string formFlowType,
			bool isReplenishmentTask)
			=>  warehouse.IsTaskManagementEnabled
				? GetNextTransferAndTaskFromWhsTaskManagementService(factory, response, warehouse, staff, formFlowType, errorMessage, isReplenishmentTask)
				: (WhsTransferHelper.GetOldestMatchingTransfer(factory, staff, criteria, clientPK, warehouse.PK, exceptedPKs, string.Empty, onlyReturnReplenishments: isReplenishmentTask), null);

		static (WhsTransfer, ProcessTask) GetNextTransferAndTaskFromWhsTaskManagementService(BusinessObjectFactory factory, WebServiceResponse response, WhsWarehouse warehouse, GlbStaff staff, string formFlowType, string errorMessage, bool isReplenishmentTask)
		{
			WhsTransfer transferToReturn = null;
			ProcessTask transferTaskToReturn = null;

			var getNextTaskResult = ObjectFactory.Get<IWhsTaskManagementService>().GetNextTaskForWarehouseWeb(factory, staff, warehouse.PK.ToGuid(), formFlowType, Array.Empty<Guid>());
			if (string.IsNullOrEmpty(getNextTaskResult.ErrorMessage))
			{
				if (!getNextTaskResult.TaskFormFlowType.Equals(formFlowType, StringComparison.OrdinalIgnoreCase))
				{
					response.LogBusinessValidationError(errorMessage);
				}
				else
				{
					var task = factory.Load<ProcessTask>(getNextTaskResult.TaskPK);
					if (task is null
						|| task.ParentBusinessObject is not WhsTransfer transfer
						|| transfer.IsFinalisedOrCancelled
						|| !IsValidTransferToProcess(transfer, isReplenishmentTask))
					{
						response.LogBusinessValidationError(errorMessage);
					}
					else
					{
						transferToReturn = transfer;
						transferTaskToReturn = task;
					}
				}
			}
			else
			{
				response.LogBusinessValidationError(getNextTaskResult.ErrorMessage);
			}

			return (transferToReturn, transferTaskToReturn);
		}

		static bool IsValidTransferToProcess(WhsTransfer transfer, bool isReplenishmentTask)
			=> !transfer.WD_IsPutawayTransfer
				&& !transfer.IsTransferringForOrder
				&& transfer.IsMasterTransfer
				&& (!isReplenishmentTask || transfer.WD_IsPickFaceReplenishment)
				&& (isReplenishmentTask || !transfer.WD_IsPickFaceReplenishment);

		#endregion

		#endregion

		static void PrepareTransferLinesAndSave(
			BusinessObjectFactory factory,
			WhsDocketWebServiceResponse response,
			SearchFilterCriteriaInfo criteria,
			GlbStaff staff,
			WhsTransfer transfer,
			ProcessTask transferTask,
			string noTransferOrLinesToReturnErrorMessage,
			string concurrencyErrorMessage,
			bool isForcedPutaway = false,
			bool canShowStockOnHandWarningOnPutaway = false)
		{
			WhsTransferHelper.SetLinesForAllocateOrPutaway(factory, response, transfer, staff, criteria, isForcedPutaway, transferTask?.PK ?? ZGuid.Empty);

			if (response.NoError())
			{
				if (transfer == null || response.Docket.Lines.Count == 0) // If transfer doesn't have any assignable lines it will still return a transfer without any lines
				{
					response.LogBusinessValidationError(noTransferOrLinesToReturnErrorMessage);
				}
				else
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(factory, response, (concurrencyException) => concurrencyErrorMessage);
					if (canShowStockOnHandWarningOnPutaway)
					{
						response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
					}
				}
			}
		}

		static string NoGetNextTransferAvailableErrorMessage => Res.GetString("243df0d8-b24e-4109-ad3b-f9964a53d8c2", "Un-finalized transfer could not be found.");

		#endregion
	}
}
