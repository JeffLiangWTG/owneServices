namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public interface ICopyable<T>
	{
		T Copy();
	}
}
