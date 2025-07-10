namespace Enterprise.Warehouse.Yard.Integration;

public interface IYardValidationRequest
{
	string? OrgCode { get; set; }
	string? AddressCode { get; set; }
	string? FacilityCode { get; set; }
	string? ReferenceNumber { get; set; }
}

public interface IYardPickupRequest : IYardValidationRequest { }

public interface IYardDropoffRequest : IYardValidationRequest
{
	bool? IsLaden { get; set; }
}
