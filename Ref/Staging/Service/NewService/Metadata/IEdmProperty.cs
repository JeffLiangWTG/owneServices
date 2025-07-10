namespace CargoWise.RefDbRepo.Staging.NewService.Metadata
{
	public interface IEdmProperty
	{
		string Name { get; }
		bool Nullable { get; }
		int? MaxLength { get; }
		string TypeName { get; }
	}
}
