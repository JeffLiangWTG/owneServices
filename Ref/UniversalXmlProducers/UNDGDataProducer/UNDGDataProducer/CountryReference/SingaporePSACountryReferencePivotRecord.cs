namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class SingaporePSACountryReferencePivotRecord
	{
		public string UNNO { get; set; }
		public string Variant { get; set; }
		public string Standard { get; set; }
		public string Type { get; set; }
		public string Country { get; set; }
		public string Code { get; set; }
		public bool HasFlashPointLower { get; set; }
		public string FlashPointLowerCentigrade { get; set; }
		public bool HasFlashPointUpper { get; set; }
		public string FlashPointUpperCentigrade { get; set; }
	}
}
