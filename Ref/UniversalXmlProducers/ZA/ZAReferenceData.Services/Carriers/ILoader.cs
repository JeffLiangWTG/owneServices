namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers
{
	public interface ILoader<T> where T : class
	{
		BaseData<T> LoadData();
	}
}
