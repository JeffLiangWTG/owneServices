namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RITADictionaryEntry : IDictionaryEntry
	{
		public RITADictionaryEntry(string code, string description, string type)
		{
			Code = code;
			Description = description;
			Type = type;
		}

		public string Code { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
	}
}
