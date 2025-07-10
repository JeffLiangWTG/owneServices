using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class IStagingDataWrapperExtensions
	{
		public static object[] GetValues(this IStagingDataWrapper wrapper, string[] propertyNames)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(propertyNames, nameof(propertyNames));
			return propertyNames.Select(wrapper.GetWrapperValue).ToArray();
		}
	}
}
