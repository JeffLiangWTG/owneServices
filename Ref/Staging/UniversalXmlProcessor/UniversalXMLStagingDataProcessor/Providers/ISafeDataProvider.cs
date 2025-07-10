using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface ISafeDataProvider
	{
		Type GetTypeFromTblPrefix(string tablePrefix);
		IEnumerable<Tuple<Type, string, string[]>> GetRelatedTypeAndNKPropertyNames(string entityName, IMetadataProvider metadataProvider);
		T GetRelatedEntity<T>((string PropertyName, object PropertyValue)[] nkPropertyNamesAndValues) where T : class;
		T Create<T>(IStagingDataWrapper wrapper, object safeParentObj, List<object> safeObjsListToSearch, IMetadataProvider metadataProvider) where T : class;
		IEnumerable<T> GetData<T>(IStagingDataWrapper[] wrappers, IMetadataProvider metadataProvider) where T : class;
		Dictionary<int, T[]> GetNewestObjectFromList<T>(object[] safeObjs, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, bool returnAllObjects) where T : class;
		T GetIdenticalObjectFromList<T>(object[] safeObjs, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider) where T : class;
		T GetSpecifiedDateTimeRangeObjectFromList<T>(object[] safeObjs, DateTimeRange dateTimeRange, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider) where T : class;
		IEnumerable<object> GetRelatedData<T>(T obj, string relatedTypeName);
		int GetIdenticalLevel<T>(T safeObj, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider);
		Task<bool> SaveChangesAsync();
		void RemoveMatchedRelatedObj<T, TRelated>(T safeObj, IStagingDataWrapper relatedWrapper, IMetadataProvider metadataProvider);
		bool ShouldOverWriteAndNotExpire<T>();
		bool IsExpirable<T>(DateTimeRange safeDateTimeRange, DateTimeRange wrapperDateTimeRange);
		void Update<T>(T safeObject, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, IdenticalLevel identicalLevel, int keyOrder = 0);
		void Expire<T>(T safeObj, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider);
		Task<bool> BatchExpire<T>(IEnumerable<Guid> safeObjPKs, DateTime expiredDate) where T : class;
		Task<bool> BatchInActive<T>(IEnumerable<Guid> safeObjPKs) where T : class;
		Task<bool> BatchDelete<T>(IEnumerable<Guid> safeObjPKs) where T : class;
		bool IsUserOverride<T>(T safeObj);
		void Add<T>(T safeObj);
		void Delete<T>(T safeObj);
		DateTimeRange GetDateTimeRange<T>(T safeObj);
		Task<IEnumerable<CloneProcessResult>> CloneExistingRecordChildrenIntoNewRecord<T>(IEnumerable<CloneProcessObject> cloneProcessObjects) where T : class;
		IEnumerable<Guid> DeleteConflictedRecordsByOrder<T>(int keyOrder, Dictionary<int, IEnumerable<object>> safeObjsDictionary) where T : class;
		void SavePersistentObjects();
		IEnumerable<object> GetAllPersistentObjects();
	}
}
