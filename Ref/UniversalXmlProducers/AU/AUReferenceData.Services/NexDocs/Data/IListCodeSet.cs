namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public interface IListCodeSet
	{
		int Position { get; }
		IItemCodeSet[] Items { get; }
	}
}
