using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.NewService.Metadata
{
	public interface IEntityType
	{
		string Name { get; }
		string KeyProperty { get; }
		IEnumerable<IEdmProperty> Properties { get; }
		IEnumerable<INavigationProperty> NavigationProperties { get; }
	}
}
