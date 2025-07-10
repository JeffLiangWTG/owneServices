using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CanTransferProductToUnexpectedScannedLocation

		[WebMethod(Description = "Check if a user can transfer product to scanned dynamic pick face location")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CanTransferProductToUnexpectedScannedLocation(Guid transferPK, string clientCode, Guid productPK, string expectedDestinationLocation, string scannedDestinationLocation)
		{
			return HandleWebServiceRequest<WebServiceResponse>(result => CanTransferProductToUnexpectedScannedLocationCore(result, transferPK, clientCode, productPK, expectedDestinationLocation, scannedDestinationLocation));
		}

		#region CanTransferProductToUnexpectedScannedLocationCore

		void CanTransferProductToUnexpectedScannedLocationCore(WebServiceResponse response, Guid transferPK, string clientCode, Guid productPK, string expectedDestinationLocation, string scannedDestinationLocation)
		{
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var proposedLocation = whs.FindLocation(scannedDestinationLocation);
			var expectedLocation = whs.FindLocation(expectedDestinationLocation);

			if (!expectedLocation.IsDynamicPickFaceLocation)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("b4213261-9a9b-4ec6-a9eb-fcb836d6d9a5", "Scanned value of '{0}' does not match expected Location. Please scan again.", scannedDestinationLocation);
			}
			else if (proposedLocation == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("2620e436-6f2d-4675-8b23-4923f3a1d74f", "Invalid Location. Scan a valid dynamic location.");
			}
			else if (!proposedLocation.IsDynamicPickFaceLocation)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("1142a06b-4df7-4127-8fad-a59560af01fd", "Not a dynamic location. Scan a dynamic location.");
			}
			else
			{
				HandleDynamicPickFaceLocation(response, transferPK, clientCode, productPK, scannedDestinationLocation, whs, proposedLocation);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		void HandleDynamicPickFaceLocation(WebServiceResponse response, Guid transferPK, string clientCode, Guid productPK, string destinationLocation, WhsWarehouse whs, WhsLocation proposedLocation)
		{
			var product = WhsProduct.GetWhsProduct(Factory, productPK);
			var transfer = Factory.Load<WhsTransfer>(transferPK);
			if (product != null)
			{
				var whsProductParamsByWhsAndClient = product.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(clientCode, whs.PK);
				if (whsProductParamsByWhsAndClient != null && proposedLocation.WLV_WA_PickingArea == whsProductParamsByWhsAndClient.W3_WA_DynamicPickFaceArea)
				{
					response.Error = ErrorTypes.YesNoEnquiry;
					response.ErrorMessage = WarehouseDataRegistry.Instance.SOHLocationWarning.Value && WebServiceHelper.GetConsumedCapacityForLocation(Factory, proposedLocation.PK) > 0m
						? Res.GetString("af30cc4e-7a5c-4d6c-abfc-a4bae893800a", "'{0}' has stock on hand and is not the expected location. Would you like to continue the transfer to this location?", destinationLocation)
						: Res.GetString("dd50e7bc-bffa-4a9d-b6b7-47359fbdc4ed", "Location '{0}' is not the expected location. Would you like to continue the transfer to this location?", destinationLocation);
				}
				else if (transfer != null && !transfer.WD_WP_PickBeingReplenished.IsEmpty)
				{
					HandlePickToBeReplenished(response, proposedLocation, destinationLocation, transfer);
				}
				else
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("7e1bc881-2dd0-438f-9092-25454bc38ca8", "Scanned location '{0}' is not assigned to dynamic area '{1}'", destinationLocation, whsProductParamsByWhsAndClient?.DynamicPickFaceArea?.WA_Name ?? string.Empty);
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("49f35e63-2f21-42b5-9675-60959c21784c", "Could not find the product from this transfer. Please seek help from support.");
			}
		}

		void HandlePickToBeReplenished(WebServiceResponse response, WhsLocation proposedLocation, string destinationLocation, WhsTransfer transfer)
		{
			var pick = Factory.Load<WhsPick>(transfer.WD_WP_PickBeingReplenished);
			var pickAreaOverride = pick?.WP_WA_DynamicPickAreaOverride ?? ZGuid.Empty;
			if (pickAreaOverride != ZGuid.Empty)
			{
				HandlePickAreaOverrideLocations(response, proposedLocation, destinationLocation, pickAreaOverride);
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("0b9515be-3430-43d8-a97f-0c201d6e4f1c", "The pick to be replenished attached to this transfer does not have a dynamic pick area override.");
			}
		}

		void HandlePickAreaOverrideLocations(WebServiceResponse response, WhsLocation proposedLocation, string destinationLocation, ZGuid pickAreaOverride)
		{
			if (pickAreaOverride == proposedLocation.WLV_WA_PickingArea)
			{
				response.Error = ErrorTypes.YesNoEnquiry;
				response.ErrorMessage = Res.GetString("ec85e36d-145f-440f-9362-89676073dc4d", "Location '{0}' is not the expected location, but is in the required dynamic area. Would you like to continue the transfer to this location?", destinationLocation);
			}
			else
			{
				var proposedArea = Factory.Load<WhsArea>(pickAreaOverride);
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("b1cb19d5-b9d6-460a-afbc-5954075c15ba", "Scanned location '{0}' is not assigned to required dynamic area '{1}'", destinationLocation, proposedArea?.WA_NameMultilingual ?? string.Empty);
			}
		}

		#endregion

		#endregion
	}
}
