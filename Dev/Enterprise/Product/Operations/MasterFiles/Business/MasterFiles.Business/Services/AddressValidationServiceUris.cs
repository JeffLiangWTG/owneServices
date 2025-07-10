namespace Enterprise.MasterFiles.Business
{
	public class AddressValidationServiceUris
	{
		public AddressValidationServiceUri Primary { get; set; }
		public AddressValidationServiceUri Secondary { get; set; }
		public AddressValidationServiceUri Background { get; set; }
	}

	public class AddressValidationServiceUri
	{
		public string Uri { get; set; } = string.Empty;
		public bool EnableS2STAuth { get; set; }
	}
}
