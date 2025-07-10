using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region GetReplenishment

		[WebMethod(Description = "Get replenishment data for pick num")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse GetReplenishment(string reference, SearchFilterCriteriaInfo criteria, Guid[] exceptedPKs)
		{
			return HandleWebServiceRequest((WhsDocketWebServiceResponse r) => GetReplenishmentCore(r, reference, criteria, exceptedPKs));
		}

		WhsDocketWebServiceResponse GetReplenishmentCore(WhsDocketWebServiceResponse response, string reference, SearchFilterCriteriaInfo criteria, Guid[] exceptedPKs)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var clientPK = WhsTransferHelper.FindClientPK(Factory, criteria.ClientCode);
			var (transfer, transferTask) = GetTransferAndAssociatedTask(
				response,
				reference,
				staff,
				warehouse,
				clientPK,
				criteria,
				exceptedPKs,
				WarehouseTaskFormFlowTypes.ReplenishmentJob,
				NoGetNextReplenishmentAvailableErrorMessage,
				isReplenishmentTask: true,
				(transferReference) => GetReplenishmentWithReference(response, clientPK, transferReference, staff, warehouse, criteria)); // If no transfer is found or there are no assignable lines on the transfer we inform the user of this.

			if (response.NoError())
			{
				if (transferTask != null)
				{
					BeginRFTaskHelper.BeginRFTask(response, transferTask, WarehouseTaskFormFlowTypes.ReplenishmentJob, staff);
				}

				if (response.NoError())
				{
					var errorMessage = reference.IsNullOrEmpty() && warehouse.IsTaskManagementEnabled
						? NoGetNextReplenishmentAvailableErrorMessage
						: Res.GetString("3be835e1-dda3-4163-813c-d5cdb31db53b", "Replenishment transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.");
					PrepareTransferLinesAndSave(
						Factory,
						response,
						criteria,
						staff,
						transfer,
						transferTask,
						errorMessage,
						concurrencyErrorMessage: WhsTransferHelper.WhsTransferConcurrencyError);
				}
			}

			return response;
		}

		#region GetReplenishmentWithReference

		WhsTransfer GetReplenishmentWithReference(WebServiceResponse response, ZGuid clientPK, string reference, GlbStaff staff, WhsWarehouse warehouse, SearchFilterCriteriaInfo criteria)
		{
			var transfer = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, clientPK, warehouse.PK, exceptedPKs: null, reference, onlyReturnReplenishments: true);
			if (transfer == null)
			{
				response.LogBusinessValidationError(Res.GetString("48449850-ee35-4057-a8c4-5a255abacfa5", "Can't find Replenishment Transfer with Pick Number/Transfer ID: {0}.", reference));
			}

			return transfer;
		}

		static string NoGetNextReplenishmentAvailableErrorMessage => Res.GetString("2d9505a3-0b98-4890-894c-62eac710b43f", "Replenishment transfer could not be found.");

		#endregion

		#endregion
	}
}
