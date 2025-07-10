using System;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IMetadataProvider
	{
		IMetadataProvider OriginalMetadataProvider { get; }
		KeyProperty[] GetKeys(string entityType);
		bool IsData(string entityType);
		bool IsMandatory(string entityType, string propertyName);
		bool ShouldCalculateIAmUnique(string entityType, string tblPrefix);
		bool EnableNullOrEmptyKeyMatching(string entityType);
		string[] GetProperties(string entityType);
		string[] GetUpdatableProperties(string entityType, int keyOrder = 0);
		Tuple<string, string>[] GetConstantPropertyNamesAndValues(string entityType);
		bool FilterData { get; }
		bool EnableExpirable { get; }
		void Validate();
	}
}
