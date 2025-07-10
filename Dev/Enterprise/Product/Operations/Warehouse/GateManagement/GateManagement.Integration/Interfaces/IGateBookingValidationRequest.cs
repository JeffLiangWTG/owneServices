namespace Enterprise.Warehouse.GateManagement.Integration
{
	public interface IGateBookingValidationRequest
	{
		public string? OrgCode { get; set; }
		public string? AddressCode { get; set; }
		public string? FacilityCode { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? ReferenceNumberType { get; set; }
		public bool? IsLaden { get; set; }
		public string? Direction {  get; set; }
		public string? FacilityTypeCode { get; set; }
	}
}
