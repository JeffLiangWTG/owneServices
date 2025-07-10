using Enterprise.Warehouse.Yard.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer;

public sealed class YardPickupRequest : IYardPickupRequest
{
	public string OrgCode { get; set; }
	public string AddressCode { get; set; }
	public string FacilityCode { get; set; }
	public string ReferenceNumber { get; set; }
}
