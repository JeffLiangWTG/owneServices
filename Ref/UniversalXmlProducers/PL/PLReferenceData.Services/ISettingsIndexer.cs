namespace CargoWise.RefDbRepo.PLReferenceData.Services
{
	public interface ISettingsIndexer
	{
		string this[string index]
		{
			get;
		}
	}
}
