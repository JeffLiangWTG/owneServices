namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public interface IItemCodeSet
	{
		string Key { get; }
		string Value { get; }
		CodeSetValueType ValueType { get; }
	}
}
