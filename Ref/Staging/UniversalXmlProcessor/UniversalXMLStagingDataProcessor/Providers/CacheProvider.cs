using System;
using System.Collections.Concurrent;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class CacheProvider : ICacheProvider
	{
		public static ICacheProvider Create()
		{
			return new CacheProvider();
		}

		CacheProvider()
		{
			RelatedEntityCache = new ConcurrentDictionary<string, Lazy<object>>();
			RelatedTypeAndNKPropertyNamesCache = new ConcurrentDictionary<string, Lazy<Tuple<Type, string, string[]>[]>>();
			SafeKeysCache = new ConcurrentDictionary<string, Lazy<KeyProperty[]>>();
			RelatedEntities = new ConcurrentDictionary<string, Lazy<object[]>>();
			MetadataCache = new ConcurrentDictionary<string, Lazy<object>>();
			TableCodeAndTypeCache = new ConcurrentDictionary<string, Lazy<Type>>();
			KeyExpirableTypeCache = new ConcurrentDictionary<string, Lazy<Type>>();
			TableNameAndTypeCache = new ConcurrentDictionary<string, Type>();
			MatchedSafeObjPKCache = new ConcurrentDictionary<string, Lazy<Guid[]>>();
		}

		public ConcurrentDictionary<string, Lazy<object>> RelatedEntityCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<Tuple<Type, string, string[]>[]>> RelatedTypeAndNKPropertyNamesCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<KeyProperty[]>> SafeKeysCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<object[]>> RelatedEntities { get; private set; }
		public ConcurrentDictionary<string, Lazy<object>> MetadataCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<Type>> TableCodeAndTypeCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<Type>> KeyExpirableTypeCache { get; private set; }
		public ConcurrentDictionary<string, Type> TableNameAndTypeCache { get; private set; }
		public ConcurrentDictionary<string, Lazy<Guid[]>> MatchedSafeObjPKCache { get; private set; }
	}
}
