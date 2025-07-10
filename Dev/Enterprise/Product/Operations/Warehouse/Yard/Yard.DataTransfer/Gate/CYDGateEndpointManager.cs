using System;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Warehouse.GateManagement.DataTransfer;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class CYDGateEndpointManager : GateManagementFacilityEndpointManager
	{
		public override IGateBookingValidationResponse ValidateBooking(IGateBookingValidationRequest request)
		{
			if (request.Direction == GateManagementConstants.TransportBookingDirections.Codes.Delivery)
			{
				var yardRequest = new YardDropoffRequest() {
					AddressCode = request.AddressCode,
					FacilityCode = request.FacilityCode,
					IsLaden = request.IsLaden,
					OrgCode = request.OrgCode,
					ReferenceNumber = request.ReferenceNumber
				};
				return ValidateBookingContainerYardCore<IYardDropoffValidationProvider, IYardDropoffRequestValidator>(yardRequest);
			}
			else
			{
				var yardRequest = new YardPickupRequest()
				{
					AddressCode = request.AddressCode,
					FacilityCode = request.FacilityCode,
					OrgCode = request.OrgCode,
					ReferenceNumber = request.ReferenceNumber
				};
				return ValidateBookingContainerYardCore<IYardPickupValidationProvider, IYardPickupRequestValidator>(yardRequest);
			}
		}

		IGateBookingValidationResponse ValidateBookingContainerYardCore<TProvider, TValidator>(IYardValidationRequest request)
			where TProvider : IYardValidationProvider
			where TValidator : IYardValidationRequestValidator
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
