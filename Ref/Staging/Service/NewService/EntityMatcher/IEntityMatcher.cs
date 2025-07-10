using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public interface IEntityMatcher
	{
		IEnumerable<MatchingCode> GetBestMatchingCodes(EntityClass entityClass, string language, bool useRecognition, bool ignoreCase, params string[] entityNames);
	}
}
