using System;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Warehouse.GateManagement.DataTransfer;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHGateEndpointManager : GateManagementFacilityEndpointManager
	{
		public override IGateBookingValidationResponse ValidateBooking(IGateBookingValidationRequest request)
		{
			if (request.Direction == GateManagementConstants.TransportBookingDirections.Codes.Delivery)
			{
				var twhRequest = new TWHValidationRequest()
				{
					AddressCode = request.AddressCode,
					FacilityCode = request.FacilityCode,
					OrgCode = request.OrgCode,
					ReferenceNumber = request.ReferenceNumber,
					ReferenceNumberType = request.ReferenceNumberType
				};
				return ValidateBookingTransitWarehouseCore<ITWHDeliveryValidationProvider, ITWHValidationRequestValidator>(twhRequest);
			}
			else
			{
				var twhRequest = new TWHValidationRequest()
				{
					AddressCode = request.AddressCode,
					FacilityCode = request.FacilityCode,
					OrgCode = request.OrgCode,
					ReferenceNumber = request.ReferenceNumber,
					ReferenceNumberType = request.ReferenceNumberType
				};
				return ValidateBookingTransitWarehouseCore<ITWHPickupValidationProvider, ITWHValidationRequestValidator>(twhRequest);
			}
		}

		IGateBookingValidationResponse ValidateBookingTransitWarehouseCore<TProvider, TValidator>(ITWHValidationRequest request)
			where TProvider : ITWHValidationProvider
			where TValidator : ITWHValidationRequestValidator
		{
			using (Db.DisposableActionForDbConnection())
			{
				var validator = ObjectFactory.Get<TValidator>();

				if (!validator.IsValid(request, out var message))
				{
					throw new ArgumentException(message);
				}

				var validationProvider = ObjectFactory.Get<TProvider>();
				var data = validationProvider.Get(request);

				return (IGateBookingValidationResponse)data;
			}
		}
	}
}
