using System;
using Enterprise.Warehouse.Yard.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class YardPickupRequestValidator : IYardPickupRequestValidator
	{
		public bool IsValid(IYardValidationRequest request, out string message)
		{
			message = request switch
			{
				null => (NoResString)"Missing request details",
				IYardPickupRequest => request switch
				{
					IYardPickupRequest pickupRequest when
							string.IsNullOrWhiteSpace(request.FacilityCode) && (string.IsNullOrWhiteSpace(pickupRequest.OrgCode) || string.IsNullOrWhiteSpace(pickupRequest.AddressCode))
							=> (NoResString)"Either facility code or organization code with address code is required",
					IYardPickupRequest pickupRequest when string.IsNullOrWhiteSpace(pickupRequest.ReferenceNumber) => (NoResString)"Reference number is missing",
					_ => null
				},
				_ => throw new ArgumentException($"Request is not type of {nameof(IYardPickupRequest)}")
			};

			return message == null;
		}
	}
}
