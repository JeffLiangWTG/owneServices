using System;
using Enterprise.Warehouse.Yard.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public class YardDropoffRequestValidator : IYardDropoffRequestValidator
	{
		public bool IsValid(IYardValidationRequest request, out string message)
		{
			message = request switch
			{
				null => (NoResString)"Missing request details",
				IYardDropoffRequest => request switch
				{
					IYardDropoffRequest dropoffRequest when
						string.IsNullOrWhiteSpace(request.FacilityCode) && (string.IsNullOrWhiteSpace(dropoffRequest.OrgCode) || string.IsNullOrWhiteSpace(dropoffRequest.AddressCode))
						=> (NoResString)"Either facility code or organization code with address code is required",
					IYardDropoffRequest dropoffRequest when string.IsNullOrWhiteSpace(dropoffRequest.ReferenceNumber) => (NoResString)"Reference number is missing",
					IYardDropoffRequest { IsLaden: null } => (NoResString)"Laden is missing",
					_ => null
				},
				_ => throw new ArgumentException($"Request is not type of {nameof(IYardDropoffRequest)}")
			};

			return message == null;
		}
	}
}
