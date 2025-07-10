namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class AdditionalTranslation
	{
		public string Code { get; set; }

		public string Description { get; set; }
	}

	public class DataParser
	{
		public string Key { get; set; }

		public AdditionalTranslation[] AdditionalTranslations { get; set; }
	}
}
