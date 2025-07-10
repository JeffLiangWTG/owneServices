using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transit.Business;

public sealed class TWHValidationRequest : ITWHValidationRequest
{
	public string OrgCode { get; set; }
	public string AddressCode { get; set; }
	public string FacilityCode { get; set; }
	public string ReferenceNumber { get; set; }
	public string ReferenceNumberType { get; set; }
}
