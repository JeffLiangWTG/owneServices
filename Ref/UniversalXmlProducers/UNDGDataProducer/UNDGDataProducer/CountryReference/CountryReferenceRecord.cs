namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CountryReferenceRecord
	{
		public string Type { get; set; }
		public string Country { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public bool HasFlashPointLower { get; set; }
		public string FlashPointLowerCentigrade { get; set; }
		public bool HasFlashPointUpper { get; set; }
		public string FlashPointUpperCentigrade { get; set; }
	}
}
