namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public interface ILocationsItem
	{
		string Location { get; }
		string Name { get; }
		string StartDate { get; }
		string EndDate { get; }
	}
}
