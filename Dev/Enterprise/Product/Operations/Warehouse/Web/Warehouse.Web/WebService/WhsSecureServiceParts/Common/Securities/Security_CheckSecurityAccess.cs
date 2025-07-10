using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Environment;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Security

		public enum RFSecurityAccessType
		{
			Unload,
			UnloadDuplicatePreviousLine,
			ASNUnload,
			Putaway,
			ReceiveFinalise,
			Picking,
			Inventory,
			Stocktake,
			Release,
			Transfers,
			AllowBadScan,
			AuthorizeBadScan,
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[WebMethod(Description = "Check if the security access is allowed for a specific type")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsSecurityAccessWebServiceResponse Security_CheckSecurityAccess(RFSecurityAccessType securityAccessType, bool requireValidateConcurencyLogin = true)
		{
			return HandleWebServiceRequest(r => Security_CheckSecurityAccess(r, securityAccessType),
				(WhsSecurityAccessWebServiceResponse t) => ValidateWebServiceAndStaffAndWarehouse(t, requireValidateConcurencyLogin));
		}

		WhsSecurityAccessWebServiceResponse Security_CheckSecurityAccess(WhsSecurityAccessWebServiceResponse response, RFSecurityAccessType securityAccessType)
		{
			Security.SecurityCheckpoint securityCheckpoint;
			switch (securityAccessType)
			{
				case RFSecurityAccessType.Unload:
					securityCheckpoint = Env.Security.WhsRFScanningUnloadEdit;
					break;
				case RFSecurityAccessType.UnloadDuplicatePreviousLine:
					securityCheckpoint = Env.Security.WhsRFScanningUnloadDuplicatePreviousLine;
					break;
				case RFSecurityAccessType.ASNUnload:
					securityCheckpoint = Env.Security.WhsRFScanningASNUnload;
					break;
				case RFSecurityAccessType.Putaway:
					securityCheckpoint = Env.Security.WhsRFScanningPutawayEdit;
					break;
				case RFSecurityAccessType.ReceiveFinalise:
					securityCheckpoint = Env.Security.WhsReceiveFinalise;
					break;
				case RFSecurityAccessType.Picking:
					securityCheckpoint = Env.Security.WhsRFScanningPickingEdit;
					break;
				case RFSecurityAccessType.Inventory:
					securityCheckpoint = Env.Security.WhsRFScanningInventoryView;
					break;
				case RFSecurityAccessType.Stocktake:
					securityCheckpoint = Env.Security.WhsRFScanningStocktakeEdit;
					break;
				case RFSecurityAccessType.Release:
					securityCheckpoint = Env.Security.WhsRFScanningReleaseEdit;
					break;
				case RFSecurityAccessType.Transfers:
					securityCheckpoint = Env.Security.WhsRFScanningTransfersEdit;
					break;
				case RFSecurityAccessType.AllowBadScan:
					securityCheckpoint = Env.Security.WhsRFScanningAllowBadScan;
					break;
				case RFSecurityAccessType.AuthorizeBadScan:
					securityCheckpoint = Env.Security.WhsRFScanningAuthorizeBadScan;
					break;
				default:
					securityCheckpoint = null;
					break;
			}

			if (securityCheckpoint != null)
			{
				response.HasAccess = securityCheckpoint.IsAllowed;
				response.Message = response.HasAccess ? string.Empty : securityCheckpoint.ErrorMessageForNotAllowed;
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("5DD0AB50-C99B-4989-BDDA-28581DA9516F", "Invalid Security Access Type!"));
			}

			return response;
		}

		#endregion
	}
}
