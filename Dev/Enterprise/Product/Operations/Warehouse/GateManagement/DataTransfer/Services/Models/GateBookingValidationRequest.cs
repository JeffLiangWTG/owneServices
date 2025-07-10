using Enterprise.Warehouse.GateManagement.Integration;
using Newtonsoft.Json;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GateBookingValidationRequest : IGateBookingValidationRequest
	{
#nullable enable
		public string? OrgCode { get; set; }
		public string? AddressCode { get; set; }
		public string? FacilityCode { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? ReferenceNumberType { get; set; }

		[JsonProperty("laden")]
		public bool? IsLaden { get; set; }
		public string? Direction { get; set; }
		public string? FacilityTypeCode { get; set; }
#nullable disable
	}
}
