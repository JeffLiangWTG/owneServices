using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class AddInfoCollectionCreator
	{
		public static List<AddInfo> CreateCollection(IColumnIndexer indexer, SchemaStringColumn addInfoColumn)
		{
			List<AddInfo> result;
			Dictionary<ZString, AddInfoDescAttribute> dictionary = null;
			var addInfoManager = indexer as IAddInfoManager;
			var bizObj = indexer as BusinessObject;
			if (addInfoManager == null)
			{
				result = CreateCollection(indexer.GetValue(addInfoColumn));
			}
			else
			{
				if (bizObj != null)
				{
					dictionary = AddInfoExtensions.AddInfoExtensions.GetAddInfoDescAttribute(bizObj);
				}

				result = new List<AddInfo>();
				var info = addInfoManager.AddInfo;
				foreach (var entry in GetSupportedEntries(info.GetKeys(), addInfoManager as IAddInfoManagerWithSchema))
				{
					var value = info.GetEffectiveValue(entry.Value.PropertyName);
					if (value != null && !value.IsEmpty && !(value is ZGuid))
					{
						var addInfo = new AddInfo()
						{
							Key = entry.Key,
							Value = GetStringValue(value)
						};

						result.Add(addInfo);
						AddInfoDescAttribute attribute;
						if (dictionary != null && dictionary.TryGetValue(entry.Key, out attribute))
						{
							if (attribute != null)
							{
								addInfo = new AddInfo()
								{
									Key = entry.Key + (NoResString)"Description",
									Value = attribute.GetDescription(bizObj)
								};

								result.Add(addInfo);
							}
						}
					}
				}
			}

			return CreateCollectionFromSyncAddInfosAndAddInfoChild(bizObj, bizObj as IAddInfoWithSyncPropertySupporter, bizObj as IAddInfoChildSupporter, result);
		}

		public static List<AddInfo> CreateCollectionFromSyncAddInfosAndAddInfoChild(BusinessObject bizObj, IAddInfoWithSyncPropertySupporter addInfoWithSyncPropertySupporter, IAddInfoChildSupporter addInfoChildSupporter, List<AddInfo> addInfos = null)
		{
			var result = addInfos;
			var addInfoChild = addInfoChildSupporter?.AddInfoChild;
			var addInfoChildSchemaDictionary = addInfoChild != null ? addInfoChildSupporter.GetAddInfoChildSchemaDictionary() : new Dictionary<string, SchemaColumn>();
			var syncAddInfos = addInfoWithSyncPropertySupporter?.GetSyncAddInfos() ?? Array.Empty<(string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)>();

			if (addInfoChildSchemaDictionary.Count > 0 || syncAddInfos.Length > 0)
			{
				var dictionary = GetSyncAddInfosAndAddInfoChildDictionaryExcludingMatchingOldAddInfo(bizObj, addInfoChildSchemaDictionary, addInfoChild, syncAddInfos, syncAddInfos.Length > 0 ? addInfoWithSyncPropertySupporter.AddInfo : null)
					.Select(x =>
					{
						var value = x.getAddInfoValue(x.propertyName, (IZType)x.bo[x.propertyName]);
						return new AddInfo { Key = x.addInfoName, Value = GetStringValue(value) };
					}).ToDictionary(x => x.Key);
				if (addInfos != null)
				{
					foreach (var addInfo in addInfos.Where(x => x.Key.HasValue))
					{
						var key = addInfo.Key.Value;
						if (dictionary.TryGetValue(key, out var newValue))
						{
							newValue.Value = addInfo.Value;
						}
						else
						{
							dictionary.Add(key, addInfo);
						}
					}
				}
				result = dictionary.Values.OrderBy(x => x.Key).ToList();
			}
			return result;
		}

		static IEnumerable<(string addInfoName, string propertyName, BusinessObject bo, Func<string, IZType, IZType> getAddInfoValue)> GetSyncAddInfosAndAddInfoChildDictionaryExcludingMatchingOldAddInfo(BusinessObject bizObj, IReadOnlyDictionary<string, SchemaColumn> addInfoChildSchemaDictionary, BusinessObject addInfoChild, (string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] syncAddInfos, IAddInfoWithSyncProperty addInfoWithSyncProperty)
		{
			Func<string, IZType, IZType> getAddInfoValue = (y, x) => x;
			IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> applicableData = addInfoChildSchemaDictionary.Select(x => (x.Key, x.Value.Name, addInfoChild));
			if (addInfoWithSyncProperty != null)
			{
				(applicableData, getAddInfoValue) = GetSyncAddInfosDataOverride(applicableData, addInfoChild, syncAddInfos, addInfoWithSyncProperty);
			}
			return GetExcludingMatchingOldAddInfo(applicableData, bizObj).Select(x => (x.addInfoName, x.propertyName, x.bo, getAddInfoValue));
		}

		static (IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> applicableData, Func<string, IZType, IZType> getAddInfoValue) GetSyncAddInfosDataOverride(IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> applicableData, BusinessObject addInfoChild, (string addInfoName, Type addInfoValueType, ZPropertyInfo info, string fastSearchName)[] syncAddInfos, IAddInfoWithSyncProperty addInfoWithSyncProperty)
		{
			Func<string, IZType, IZType> getAddInfoValue = (y, x) => x;
			if (syncAddInfos.Length > 0)
			{
				var propertiesMapping = applicableData.ToDictionary(x => x.propertyName, y => (y.addInfoName, y.bo));
				var dictionary = new Dictionary<string, (string propertyName, BusinessObject bo)>();
				var propertyToAddInfoValueTypeMapping = new Dictionary<string, Type>();
				foreach ((string addInfoName, Type addInfoValueType, ZPropertyInfo info, _) in syncAddInfos)
				{
					var bizObj = info is ZWrappedPropertyInfo wrappedPropertyInfo ? wrappedPropertyInfo.InnerInfo.BizObj : info.BizObj;
					if (ReferenceEquals(bizObj, addInfoChild))
					{
						var propertyName = info.Name;
						if (propertiesMapping.ContainsKey(propertyName))
						{
							dictionary.Add(addInfoName, (propertyName, info.BizObj));
							propertyToAddInfoValueTypeMapping.Add(propertyName, addInfoValueType);
							propertiesMapping.Remove(propertyName);
						}
					}
					else
					{
						dictionary.Add(addInfoName, (info.Name, info.BizObj));
						propertyToAddInfoValueTypeMapping.Add(info.Name, addInfoValueType);
					}
				}
				foreach (var data in propertiesMapping)
				{
					var addInfoName = data.Value.addInfoName;
					if (!dictionary.ContainsKey(addInfoName))
					{
						dictionary.Add(addInfoName, (data.Key, data.Value.bo));
					}
				}

				getAddInfoValue = (y, x) => propertyToAddInfoValueTypeMapping.TryGetValue(y, out var addInfoValueType) ? addInfoWithSyncProperty.GetAddInfoValue(x, addInfoValueType) : x;
				applicableData = dictionary.Select(x => (x.Key, x.Value.propertyName, x.Value.bo));
			}
			return (applicableData, getAddInfoValue);
		}

		static IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> GetExcludingMatchingOldAddInfo(IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> dictionary, BusinessObject bizObj)
		{
			IEnumerable<(string addInfoName, string propertyName, BusinessObject bo)> applicableData = dictionary;
			if (bizObj is IAddInfoManagerWithSchema addInfoManagerWithSchema)
			{
				var addInfoSchema = addInfoManagerWithSchema.AddInfoSchema;
				var addInfoSchemaDictionary = bizObj.Factory.GetAddInfoSchemaDictionary(bizObj.GetType(), addInfoSchema);
				applicableData = dictionary.Where(x => !addInfoSchemaDictionary.ContainsKey(x.addInfoName));
			}
			else if (bizObj is IAddInfoManager addInfoManager)
			{
				var addInfo = addInfoManager.AddInfo;
				var keys = addInfo.GetKeys();
				applicableData = dictionary.Where(x => !keys.ContainsKey(x.addInfoName));
			}
			return applicableData;
		}

		static string GetStringValue(IZType value)
		{
			var stringValue = BaseAddInfo.GetStringRepresentation(value);
			if (string.IsNullOrEmpty(stringValue) && value is ZBool)
			{
				stringValue = "N";
			}
			return stringValue;
		}

		static IEnumerable<(ZString Key, AddInfoPropertyNameAndValueParser Value)> GetSupportedEntries(IDictionary<ZString, AddInfoPropertyNameAndValueParser> addInfoKeys, IAddInfoManagerWithSchema addInfoManager)
		{
			if (addInfoManager == null)
			{
				foreach (var addInfoKey in addInfoKeys)
				{
					yield return (addInfoKey.Key, addInfoKey.Value);
				}
			}
			else
			{
				foreach (var addInfo in addInfoManager.Factory.GetAddInfoSchemaDictionary(addInfoManager.GetType(), addInfoManager.AddInfoSchema))
				{
					if (addInfoKeys.TryGetValue(addInfo.Key, out var parser))
					{
						yield return (addInfo.Key, parser);
					}
				}
			}
		}
		public static List<AddInfo> CreateCollection(ZString addInfoString)
		{
			var result = new List<AddInfo>();
			if (!addInfoString.IsEmpty)
			{
				foreach (var entry in AddInfoParser.CreateDictionaryWithAddInfoString(addInfoString))
				{
					if (!IsGuidValue(entry.Value))
					{
						var addInfo = new AddInfo()
						{
							Key = entry.Key,
							Value = entry.Value
						};

						result.Add(addInfo);
					}
				}
			}

			return result.Count == 0 ? null : result;
		}

		public static bool IsGuidValue(ZString value)
		{
			bool result = value.Length == 36;
			if (result)
			{
				ZGuid pk;
				result = ZGuid.TryParse(value, out pk);
			}

			return result;
		}
	}
}
