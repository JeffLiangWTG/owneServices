using System.Linq;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class DescriptionAsMainContentList
	{
		public static bool Contains(string entityType)
		{
			var descriptionAsMainContentList = Constants.DescriptionAsMainContentList;
			if (!string.IsNullOrEmpty(descriptionAsMainContentList))
			{
				var entityTypeList = descriptionAsMainContentList.Split(';');
				return entityTypeList.Contains(entityType);
			}
			return false;
		}
	}
}
