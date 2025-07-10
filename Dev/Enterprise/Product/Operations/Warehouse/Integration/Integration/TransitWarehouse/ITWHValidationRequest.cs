namespace Enterprise.Warehouse.Integration
{
	public interface ITWHValidationRequest
	{
		string OrgCode { get; set; }
		string AddressCode { get; set; }
		string FacilityCode { get; set; }
		string ReferenceNumber { get; set; }
		string ReferenceNumberType { get; set; }
	}
}
