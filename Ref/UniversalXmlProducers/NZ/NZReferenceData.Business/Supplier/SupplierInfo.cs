namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public class SupplierInfo
	{
		public string Code { get; set; }

		public string Name { get; set; }

		public string CountryCode { get; set; }

		public bool IsValid => !(string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(CountryCode));
	}
}
