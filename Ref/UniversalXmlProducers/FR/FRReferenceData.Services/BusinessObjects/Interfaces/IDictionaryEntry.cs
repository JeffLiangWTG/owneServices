namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IDictionaryEntry
	{
		string Code { get; set; }
		string Description { get; set; }
		string Type { get; set; }
	}
}
