using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region BeginRFTransferTask

		[WebMethod(Description = "Begin RF Transfer Task")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse BeginRFTransferTask(Guid taskPK, bool isReplenishmentTask)
		{
			return HandleWebServiceRequest((WhsDocketWebServiceResponse r) => BeginRFTransferTaskCore(r, taskPK, isReplenishmentTask));
		}

		void BeginRFTransferTaskCore(WhsDocketWebServiceResponse response, Guid taskPK, bool isReplenishmentTask)
		{
			var task = Factory.Load<ProcessTask>(taskPK);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

			PrepareTaskForTransfer(response, task, staff, isReplenishmentTask);
		}

		void PrepareTaskForTransfer(WhsDocketWebServiceResponse response, ProcessTask task, GlbStaff staff, bool isReplenishmentTask)
		{
			var expectedFormFlowType = isReplenishmentTask ? WarehouseTaskFormFlowTypes.ReplenishmentJob : WarehouseTaskFormFlowTypes.TransferJob;
			BeginRFTaskHelper.BeginRFTask(response, task, expectedFormFlowType, staff);

			if (response.NoError())
			{
				var transfer = Factory.Load<WhsTransfer>(task.P9_ParentID);
				if (transfer == null)
				{
					response.LogBusinessValidationError(Res.GetString("d7ab1098-9cb1-4354-aefa-ce965573cb4b", "Transfer is not found."));
				}
				else if (transfer.IsFinalisedOrCancelled)
				{
					response.LogBusinessValidationError(Res.GetString("921c84e1-4161-45e7-b248-28a19d302724", "Transfer is already finalized or canceled."));
				}
				else if (!IsValidTransferToProcess(transfer, isReplenishmentTask))
				{
					response.LogBusinessValidationError(BeginRFTaskHelper.InvalidTaskType);
				}
				else
				{
					var errorMessage = isReplenishmentTask
						? Res.GetString("091d3560-ccb6-4d4c-afc2-19e9ed523f35", "Un-finalized replenishment transfer could not be found for this task.")
						: Res.GetString("a0e87e88-0d50-4e9d-8b56-b9a222472e36", "Un-finalized transfer could not be found for this task.");

					PrepareTransferLinesAndSave(
						Factory,
						response,
						new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" },
						staff,
						transfer,
						task,
						errorMessage,
						concurrencyErrorMessage: BeginRFTaskHelper.TaskConcurrencyErrorMessage,
						canShowStockOnHandWarningOnPutaway: !isReplenishmentTask);
				}
			}
		}

		#endregion
	}
}
