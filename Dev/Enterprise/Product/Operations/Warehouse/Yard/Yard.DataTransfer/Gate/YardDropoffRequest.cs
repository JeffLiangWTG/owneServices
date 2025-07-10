using Enterprise.Warehouse.Yard.Integration;
using Newtonsoft.Json;

namespace Enterprise.Warehouse.Yard.DataTransfer;

public sealed class YardDropoffRequest : IYardDropoffRequest
{
	public string OrgCode { get; set; }
	public string AddressCode { get; set; }
	public string FacilityCode { get; set; }
	public string ReferenceNumber { get; set; }
	[JsonProperty("laden")]
	public bool? IsLaden { get; set; }
}
