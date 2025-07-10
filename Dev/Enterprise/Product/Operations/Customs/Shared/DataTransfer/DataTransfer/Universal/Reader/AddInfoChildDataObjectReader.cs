using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AddInfoChildDataObjectReader : CommonAddInfoDataObjectReader
	{
		public AddInfoChildDataObjectReader(IXmlImportLogger logger, UniversalCommonReaderHelper helper, string[] addInfosThatShouldNotBeImported = null)
			: base(logger, helper, addInfosThatShouldNotBeImported)
		{
		}

		public void ReadIntoBusinessObject(IAddInfoChildSupporter supporter, IEnumerable<AddInfo> addInfoCollection, Dictionary<string, ValueSetter> delaySetters, IOrganizationAddressCollectionParent organizationAddressContainer = null)
		{
			var addInfoChild = supporter?.AddInfoChild;
			if (addInfoChild != null)
			{
				var applicableAddInfos = GetApplicableAddInfos(addInfoCollection);
				if (applicableAddInfos.Any() || organizationAddressContainer != null)
				{
					var dictionary = GetApplicableAddInfoChildSchemaDictionary(supporter, addInfoChild);
					if (dictionary.Count > 0)
					{
						ReadCore(addInfoChild, dictionary, applicableAddInfos, organizationAddressContainer, delaySetters, supporter.ChildForeignKeyColumn.TableSchema.PK);
					}
				}
			}
		}

		IReadOnlyDictionary<string, SchemaColumn> GetApplicableAddInfoChildSchemaDictionary(IAddInfoChildSupporter supporter, BusinessObject addInfoChild)
		{
			return supporter.Factory.GetCachedValue("ApplicableAddInfoChildSchemaDictionary|" + supporter.GetType(), () =>
			{
				var result = new Dictionary<string, SchemaColumn>();
				var dictionary = supporter.GetAddInfoChildSchemaDictionary();
				var getAlternateAddInfoName = new Func<string, string, string>((addInfoName, propertyName) => addInfoName);
				if (dictionary.Count > 0)
				{
					var addInfoWithSyncPropertySupporter = supporter as IAddInfoWithSyncPropertySupporter;
					var addInfoChildSyncProperties = GetAddInfoChildSyncProperties(addInfoWithSyncPropertySupporter, addInfoChild);
					if (addInfoChildSyncProperties.Count > 0)
					{
						getAlternateAddInfoName = new Func<string, string, string>((addInfoName, propertyName) =>
						{
							if (addInfoChildSyncProperties.TryGetValue(propertyName, out var alternateAddInfoName))
							{
								addInfoName = alternateAddInfoName;
							}
							return addInfoName;
						});
					}
				}
				foreach (var data in dictionary)
				{
					var schemaColumn = data.Value;
					result.Add(getAlternateAddInfoName(data.Key, schemaColumn.Name), schemaColumn);
				}
				return result;
			});
		}

		IReadOnlyDictionary<string, string> GetAddInfoChildSyncProperties(IAddInfoWithSyncPropertySupporter addInfoWithSyncPropertySupporter, BusinessObject addInfoChild)
		{
			var result = new Dictionary<string, string>();
			var syncAddInfos = addInfoWithSyncPropertySupporter?.GetSyncAddInfos();
			if (syncAddInfos != null)
			{
				foreach ((string addInfoName, _, ZPropertyInfo info, _) in syncAddInfos)
				{
					var wrappedPropertyInfo = info as ZWrappedPropertyInfo;
					var infoToCheck = wrappedPropertyInfo?.InnerInfo ?? info;
					var bizObj = infoToCheck.BizObj;
					if (ReferenceEquals(addInfoChild, bizObj))
					{
						var key = infoToCheck.Name;
						result.Add(key, addInfoName);
					}
				}
			}
			return result;
		}

		void ReadCore(IColumnIndexer addInfoChild, IReadOnlyDictionary<string, SchemaColumn> addInfoChildSchemaDictionary, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn pkColumn)
		{
			ReadCollection(addInfoChildSchemaDictionary, addInfoCollection, organizationAddresContainer, (schemaColumn, value) =>
			{
				try
				{
					SetValue(addInfoChild, schemaColumn, value, delaySetters, pkColumn);
				}
				catch (ZTypeValueException ex)
				{
					logger.Log(Integration.LogType.Error, string.Format("PropertyName: {0}, Value: {1} - {2}", schemaColumn.Name, value, ex.Message));
				}
			});
		}

		void ReadCollection(IReadOnlyDictionary<string, SchemaColumn> addInfoChildSchemaDictionary, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, Action<SchemaColumn, ZString> setValue)
		{
			if (addInfoCollection != null)
			{
				ReadAddInfoCollection(addInfoChildSchemaDictionary, addInfoCollection, setValue);
			}
			if (organizationAddresContainer != null)
			{
				ReadOrganizationAddressCollection(organizationAddresContainer, setValue);
			}
		}

		protected virtual void ReadOrganizationAddressCollection(IOrganizationAddressCollectionParent organizationAddresContainer, Action<SchemaColumn, ZString> setValue)
		{
		}

		void ReadAddInfoCollection(IReadOnlyDictionary<string, SchemaColumn> addInfoChildSchemaDictionary, IEnumerable<AddInfo> addInfoCollection, Action<SchemaColumn, ZString> setValue)
		{
			var addInfoDictionary = addInfoCollection.Select(x => (x.Key.GetValueOrDefault(), x.Value)).Where(p => !p.Item1.IsEmpty).ToDictionary((k) => k.Item1, (e) => e.Value);
			foreach (var pair in addInfoChildSchemaDictionary)
			{
				if (addInfoDictionary.TryGetValue(pair.Key, out var value))
				{
					setValue(pair.Value, value.GetValueOrDefault());
				}
			}
		}
	}
}
