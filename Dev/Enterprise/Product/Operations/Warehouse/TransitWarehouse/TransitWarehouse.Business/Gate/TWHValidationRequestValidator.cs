using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHValidationRequestValidator : ITWHValidationRequestValidator
	{
		public bool IsValid(ITWHValidationRequest request, out string message)
		{
			message = request switch
			{
				null => (NoResString)"Missing request details",
				ITWHValidationRequest => request switch
				{
					ITWHValidationRequest deliveryRequest when string.IsNullOrWhiteSpace(deliveryRequest.FacilityCode) && (string.IsNullOrWhiteSpace(deliveryRequest.OrgCode) || string.IsNullOrWhiteSpace(deliveryRequest.AddressCode)) => (NoResString)"Either facility code or organization code with address code is required",
					ITWHValidationRequest deliveryRequest when string.IsNullOrWhiteSpace(deliveryRequest.ReferenceNumber) => (NoResString)"Reference number is required in the request",
					_ => null
				}
			};

			return message == null;
		}
	}
}
