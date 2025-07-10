namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class CUSTranslatedName
	{
		public CUSTranslatedName(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; }

		public string Description { get; }
	}
}
