using System.Linq;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class CloneBlackList
	{
		public static bool Contains(string subSource)
		{
			var blackList = Constants.CloneBlackList;
			if (!string.IsNullOrEmpty(blackList))
			{
				var subSourcesInBlackList = blackList.Split(';');
				return subSourcesInBlackList.Contains(subSource);
			}
			return false;
		}
	}
}
