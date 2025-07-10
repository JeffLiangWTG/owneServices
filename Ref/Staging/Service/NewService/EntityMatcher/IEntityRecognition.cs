using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public interface IEntityRecognition
	{
		IEnumerable<string> GetNamedEntities(string text);
	}
}
