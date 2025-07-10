using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IStagingDataWrapper
	{
		object GetWrapperValue(string propertyName);
		T GetWrapperValue<T>(string propertyName);
		void SetWrapperValue(string propertyName, object value);
		IEnumerable<Tuple<string, string>> GetRelatedEntityTypesAndFKs(bool includeExpirableKeys = false);
		IEnumerable<IStagingDataWrapper> GetRelatedEntities(string relatedTypeName);
		void SetRelatedEntities(Type relatedType, IEnumerable<IStagingDataWrapper> relatedWrappers);
		bool IsData { get; }
		Type GetStagingType();
		string GetStagingTypeName();
		DateTime GetWrapperStartDateTime(string tablePrefix);
		DateTime GetWrapperEndDateTime(string tablePrefix);
		DateTimeRange GetDateTimeRange();
		Guid GetWrapperOriginalPK();
	}
}
