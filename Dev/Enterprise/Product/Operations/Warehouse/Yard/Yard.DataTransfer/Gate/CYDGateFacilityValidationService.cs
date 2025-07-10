using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.DataTransfer.Universal;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class CYDGateFacilityValidationService : IGateFacilityValidationService
	{
		public string[] ValidateMovement(ITopLevelDataObject shipment, string validationType)
		{
			var validationResults = new List<string>();
			if (shipment is UniversalShipment shipmentDataObject && shipmentDataObject.SubShipmentCollection.Count > 0)
			{
				if (validationType == FacilityValidationTypes.GateIn)
				{
					validationResults.AddRange(ValidationGateIn(shipmentDataObject));
				}
			}
			else
			{
				validationResults.Add(Res.GetString("1573bd04-77f6-4467-b710-7666d6ac10ba", "Invalid shipment or validation type"));
			}
			return validationResults.ToArray();
		}

		List<string> ValidationGateIn(UniversalShipment shipmentDataObject)
		{
			var valMsg = new List<string>();

			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateTransportOrgAddress);
			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateVehicle);
			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateWaitingBayLocation);
			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateContainerCollections);
			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateBookingConfirmationReference);
			TryValidate(shipmentDataObject, valMsg, GateValidationServiceHelper.ValidateContainerInfo);

			return valMsg;
		}

		void TryValidate(UniversalShipment shipmentDataObject, List<string> valMsg, Action<UniversalShipment> validateAction)
		{
			try
			{
				validateAction(shipmentDataObject);
			}
			catch (DataObjectReadFailureException ex)
			{
				valMsg.Add(ex.Message);
			}
		}
	}
}
