using System;
using System.Collections.Concurrent;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface ICacheProvider
	{
		ConcurrentDictionary<string, Lazy<object>> RelatedEntityCache { get; }
		ConcurrentDictionary<string, Lazy<Tuple<Type, string, string[]>[]>> RelatedTypeAndNKPropertyNamesCache { get; }
		ConcurrentDictionary<string, Lazy<KeyProperty[]>> SafeKeysCache { get; }
		ConcurrentDictionary<string, Lazy<object[]>> RelatedEntities { get; }
		ConcurrentDictionary<string, Lazy<object>> MetadataCache { get; }
		ConcurrentDictionary<string, Lazy<Type>> TableCodeAndTypeCache { get; }
		ConcurrentDictionary<string, Lazy<Type>> KeyExpirableTypeCache { get; }
		ConcurrentDictionary<string, Type> TableNameAndTypeCache { get; }
		ConcurrentDictionary<string, Lazy<Guid[]>> MatchedSafeObjPKCache { get; }
	}
}
