using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class SafeDataProviderExtensions
	{
		public static IEnumerable<object> GetData(this ISafeDataProvider safeDataProvider, Type safeObjType, IStagingDataWrapper[] wrappers, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrappers, nameof(wrappers));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			return ((IEnumerable)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetData), safeObjType, (object)wrappers, metadataProvider))?
					.Cast<object>().ToArray();
		}

		public static int GetIdenticalLevel(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetIdenticalLevel), safeObjType, safeObj, wrapper, metadataProvider);
			return (int)result;
		}

		public static void RemoveMatchedRelatedObj(this ISafeDataProvider safeDataProvider, Type safeObjType, Type safeRelatedType, object safeObj, IStagingDataWrapper relatedWrapper, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(relatedWrapper, nameof(relatedWrapper));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.RemoveMatchedRelatedObj), new[] { safeObjType, safeRelatedType }, safeObj, relatedWrapper, metadataProvider);
		}

		public static bool ShouldOverWriteAndNotExpire(this ISafeDataProvider safeDataProvider, Type safeObjType)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.ShouldOverWriteAndNotExpire), safeObjType);
			return (bool)result;
		}

		public static bool IsExpirable(this ISafeDataProvider safeDataProvider, Type safeObjType, DateTimeRange safeDateTimeRange, DateTimeRange wrapperDateTimeRange)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.IsExpirable), safeObjType, safeDateTimeRange, wrapperDateTimeRange);
			return (bool)result;
		}

		public static void Expire(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.Expire), safeObjType, safeObj, wrapper, metadataProvider);
		}

		public static object Create(this ISafeDataProvider safeDataProvider, Type safeObjType, IStagingDataWrapper wrapper, object safeParentObj, List<object> safeObjsListToSearch, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.Create), safeObjType, wrapper, safeParentObj, safeObjsListToSearch, metadataProvider);
			return result;
		}

		public static void Update(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, IdenticalLevel identicalLevel, int keyOrder = 0)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.Update), safeObjType, safeObj, wrapper, metadataProvider, identicalLevel, keyOrder);
		}

		public static void Add(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObj, nameof(safeObj));
			safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.Add), safeObjType, safeObj);
		}

		public static void Delete(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObj, nameof(safeObj));
			safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.Delete), safeObjType, safeObj);
		}

		public static IEnumerable<object> GetRelatedData(this ISafeDataProvider safeDataProvider, object safeObj, Type safeObjType, string relatedTypeName)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNullOrEmpty(relatedTypeName, nameof(relatedTypeName));
			Argument.NotNull(safeObj, nameof(safeObj));
			var result = (IEnumerable<object>)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetRelatedData), safeObjType, safeObj, relatedTypeName);
			return result;
		}

		public static Dictionary<int, IEnumerable<object>> GetNewestObjectFromList(this ISafeDataProvider safeDataProvider, object[] safeObjs, Type safeObjType, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, bool returnAllObjects = false)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObjs, nameof(safeObjs));
			var result = (IDictionary)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetNewestObjectFromList), safeObjType, safeObjs, wrapper, metadataProvider, returnAllObjects);
			var newestObjectDictionary = new Dictionary<int, IEnumerable<object>>();
			if (result != null)
			{
				foreach (var key in result.Keys)
				{
					var order = (int)key;
					newestObjectDictionary[order] = (IEnumerable<object>)result[order];
				}
			}
			return newestObjectDictionary;
		}

		public static object GetSpecifiedDateTimeRangeObjectFromList(this ISafeDataProvider safeDataProvider, object[] safeObjs, Type safeObjType, DateTimeRange dateTimeRange, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObjs, nameof(safeObjs));
			Argument.NotNull(dateTimeRange, nameof(dateTimeRange));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetSpecifiedDateTimeRangeObjectFromList), safeObjType, safeObjs, dateTimeRange, wrapper, metadataProvider);
			return result;
		}

		public static object GetIdenticalObjectFromList(this ISafeDataProvider safeDataProvider, object[] safeObjs, Type safeObjType, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObjs, nameof(safeObjs));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			return safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetIdenticalObjectFromList), safeObjType, (object)safeObjs, wrapper, metadataProvider);
		}

		public static Task<bool> BatchExpire(this ISafeDataProvider safeDataProvider, string tblPrefix, IEnumerable<Guid> safeObjPKs, DateTime expiredDate)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));

			var safeObjType = safeDataProvider.GetTypeFromTblPrefix(tblPrefix);
			return (Task<bool>)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.BatchExpire), safeObjType, safeObjPKs, expiredDate);
		}

		public static Task<bool> BatchInActive(this ISafeDataProvider safeDataProvider, string tblPrefix, IEnumerable<Guid> safeObjPKs)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));

			var safeObjType = safeDataProvider.GetTypeFromTblPrefix(tblPrefix);
			return (Task<bool>)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.BatchInActive), safeObjType, safeObjPKs);
		}

		public static Task<bool> BatchDelete(this ISafeDataProvider safeDataProvider, string tblPrefix, IEnumerable<Guid> safeObjPKs)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));

			var safeObjType = safeDataProvider.GetTypeFromTblPrefix(tblPrefix);
			return (Task<bool>)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.BatchDelete), safeObjType, safeObjPKs);
		}

		public static bool IsUserOverride(this ISafeDataProvider safeDataProvider, Type safeObjType, object safeObj)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.IsUserOverride), safeObjType, safeObj);
			return (bool)result;
		}

		public static DateTimeRange GetDateTimeRange(this ISafeDataProvider safeDataProvider, Type safeObjType, object safeObj)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObj, nameof(safeObj));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.GetDateTimeRange), safeObjType, safeObj);
			return (DateTimeRange)result;
		}

		public static Task<IEnumerable<CloneProcessResult>> CloneExistingRecordChildrenIntoNewRecord(this ISafeDataProvider safeDataProvider, string tblPrefix, IEnumerable<CloneProcessObject> cloneProcessObjects)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));

			var safeObjType = safeDataProvider.GetTypeFromTblPrefix(tblPrefix);
			return (Task<IEnumerable<CloneProcessResult>>)safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.CloneExistingRecordChildrenIntoNewRecord), safeObjType, cloneProcessObjects);
		}

		public static IEnumerable<Guid> DeleteConflictedRecordsByOrder(this ISafeDataProvider safeDataProvider, Type safeObjType, int keyOrder, Dictionary<int, IEnumerable<object>> safeObjsDictionary)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObjsDictionary, nameof(safeObjsDictionary));
			var result = safeDataProvider.InvokeGenericMethod(nameof(ISafeDataProvider.DeleteConflictedRecordsByOrder), safeObjType, keyOrder, safeObjsDictionary);
			return (IEnumerable<Guid>)result;
		}
	}
}
