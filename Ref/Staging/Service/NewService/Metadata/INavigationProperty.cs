namespace CargoWise.RefDbRepo.Staging.NewService.Metadata
{
	public interface INavigationProperty
	{
		string Name { get; }
		string ToTypeName { get; }
		string FromPropertyName { get; }
		bool IsToMany { get; }
		bool IsToOne { get; }
		bool IsToZeroOrOne { get; }
	}
}
