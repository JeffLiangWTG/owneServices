using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ChangeTrolleyJobStatus

		[WebMethod(Description = "Changes trolley job status.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ChangeTrolleyJobStatus(Guid trolleyJobPK, TrolleyJobStatus newStatus)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ChangeTrolleyJobStatus(r, trolleyJobPK, newStatus));
		}

		void ChangeTrolleyJobStatus(WebServiceResponse response, Guid trolleyJobPK, TrolleyJobStatus newStatus)
		{
			var trolleyJob = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);
			if (trolleyJob == null)
			{
				response.ErrorMessage = Res.GetString("6023d4b3-eae3-4616-bce5-c21ef628a36c", "Trolley job was not found. Perhaps it was deleted.");
			}
			else if (!IsValidTrolleyJobStatusTransition(trolleyJob.WTJ_Status, newStatus))
			{
				response.ErrorMessage = Res.GetString("599ab049-3b57-4f20-af56-5bfd8dfa3291", "Cannot change status to '{0}'. Current status is '{1}'.", newStatus, trolleyJob.WTJ_Status);
			}
			else
			{
				trolleyJob.WTJ_Status = GetNewStatus(newStatus);
				var concurrencyErrorMessage = Res.GetString("d6a5a8c0-2eb5-4d03-b3ae-9bebee73ef7f", "Another user has changed trolley job status. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		ZString GetNewStatus(TrolleyJobStatus newStatus)
		{
			switch (newStatus)
			{
				case TrolleyJobStatus.Picking:
					return PickTrolleyStatus.Codes.Picking;
				case TrolleyJobStatus.Finalised:
					return PickTrolleyStatus.Codes.Finalised;
				default:
					throw new NotSupportedException("TrolleyJobStatus " + newStatus + " is not supported.");
			}
		}

		bool IsValidTrolleyJobStatusTransition(string currentStatus, TrolleyJobStatus newStatus)
		{
			return ((currentStatus == PickTrolleyStatus.Codes.Building && newStatus == TrolleyJobStatus.Picking)
					|| (currentStatus == PickTrolleyStatus.Codes.Picking && newStatus == TrolleyJobStatus.Finalised));
		}

		#endregion
	}
}
