using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AddInfoDataObjectReader<T> : BusinessObjectAddInfoDataObjectReader
		where T : BusinessObject
	{
		public AddInfoDataObjectReader(IXmlImportLogger logger, UniversalCommonReaderHelper helper, SchemaStringColumn addInfoColumn, ITableSchema tableSchema, string[] addInfosThatShouldNotBeImported = null, Dictionary<string, SchemaColumn> addInfoColumnsToDBColumnsMapping = null)
			: base(typeof(T), logger, helper, addInfoColumn, tableSchema, addInfosThatShouldNotBeImported, addInfoColumnsToDBColumnsMapping)
		{
		}
	}

	public class BusinessObjectAddInfoDataObjectReader : AddInfoDataObjectReader
	{
		public BusinessObjectAddInfoDataObjectReader(Type businessObjectType, IXmlImportLogger logger, UniversalCommonReaderHelper helper, SchemaStringColumn addInfoColumn, ITableSchema tableSchema, string[] addInfosThatShouldNotBeImported = null, Dictionary<string, SchemaColumn> addInfoColumnsToDBColumnsMapping = null)
			: base(logger, helper, addInfoColumn, addInfosThatShouldNotBeImported, addInfoColumnsToDBColumnsMapping)
		{
			this.tableSchema = Argument.NotNull(tableSchema, "tableSchema");
			this.businessObjectType = Argument.NotNull(businessObjectType, nameof(businessObjectType));
		}

		protected override IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetInfoMappings(IAddInfoManager addInfoManager)
		{
			return helper.GetSupportedAddInfoList(businessObjectType, tableSchema);
		}

		protected override void SetValue(IAddInfoManager addInfoManager, IColumnIndexer row, ZString key, ZString value, string propertyName, Dictionary<string, ValueSetter> delaySetters)
		{
			SchemaColumn dbColumn;
			if (addInfoColumnsToDBColumnsMapping != null && addInfoColumnsToDBColumnsMapping.TryGetValue(key, out dbColumn)) // This code will be removed at 2018-08-30
			{
				if (dbColumn != null)
				{
					SetValue(row, dbColumn, value, delaySetters, addInfoColumn.TableSchema.PK);
				}
			}
			else
			{
				var addInfoSchemas = helper.Factory.BOFactory.GetAddInfoSchemaWithMaxLengthDictionary(businessObjectType, tableSchema);
				SchemaColumnAndMaxLength dataType;
				if (addInfoSchemas.TryGetValue(key, out dataType))
				{
					SetValue(row, dataType.Column, value, delaySetters, addInfoColumn.TableSchema.PK);
				}
				else
				{
					base.SetValue(addInfoManager, row, key, value, propertyName, delaySetters);
				}
			}
		}

		protected readonly ITableSchema tableSchema;
		readonly Type businessObjectType;
	}

	public abstract class CommonAddInfoDataObjectReader : DataObjectReader, IOrganisationDataObjectReaderSupporter
	{
		public CommonAddInfoDataObjectReader(IXmlImportLogger logger, UniversalCommonReaderHelper helper, string[] addInfosThatShouldNotBeImported = null)
			: base(logger)
		{
			this.helper = helper;
			this.addInfosThatShouldNotBeImported = addInfosThatShouldNotBeImported;
		}

		protected readonly UniversalCommonReaderHelper helper;
		protected readonly string[] addInfosThatShouldNotBeImported;

		protected IEnumerable<AddInfo> GetApplicableAddInfos(IEnumerable<AddInfo> list)
		{
			var result = Enumerable.Empty<AddInfo>();
			if (list != null)
			{
				if (addInfosThatShouldNotBeImported == null)
				{
					result = list;
				}
				else
				{
					result = list.Where(x => !addInfosThatShouldNotBeImported.Contains(x.Key.GetValueOrDefault().ToString())).ToArray();
				}
			}
			return AddAddionalAddInfo(result, list);
		}

		protected virtual IEnumerable<AddInfo> AddAddionalAddInfo(IEnumerable<AddInfo> result, IEnumerable<AddInfo> list)
		{
			return result;
		}

		protected virtual void SetAddressPK(Action<ZString, ZString, string> setValue, ZPropertyInfo info, IOrganizationAddressCollectionParent dataObject, ZString addressType, OrganisationTypes orgCategory, bool allowUnmatchedOrganisation = false)
		{
			ZGuid organisationPK;
			ZGuid addressPK;
			if (this.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, info.BizObj, addressType, orgCategory) && addressPK.IsValid)
			{
				var propertyName = info.Name;
				addressPK = !allowUnmatchedOrganisation && organisationPK == OrgHeader.UnmatchedOrganisationPK ? ZGuid.Empty : addressPK;
				setValue(propertyName.Substring(3), BaseAddInfo.GetStringRepresentation(addressPK), propertyName);
			}
		}

		protected virtual void SetOrganisationPK(Action<ZString, ZString, string> setValue, ZPropertyInfo info, IOrganizationAddressCollectionParent dataObject, ZString addressType, OrganisationTypes orgCategory, bool allowUnmatchedOrganisation = false)
		{
			ZGuid organisationPK;
			ZGuid addressPK;
			if (this.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, info.BizObj, addressType, orgCategory) && organisationPK.IsValid)
			{
				var propertyName = info.Name;
				organisationPK = !allowUnmatchedOrganisation && organisationPK == OrgHeader.UnmatchedOrganisationPK ? ZGuid.Empty : organisationPK;
				setValue(propertyName.Substring(3), BaseAddInfo.GetStringRepresentation(organisationPK), propertyName);
			}
		}

		protected void SetValue(IColumnIndexer row, SchemaColumn column, ZString value,
			Dictionary<string, ValueSetter> delaySetters, SchemaPKColumn pkColumn)
		{
			// should be based on the order of most common type of column
			var zValue = BaseAddInfo.ConvertToZType(column.GetEquivalentZType(), value);
			if (column is SchemaStringColumn stringColumn)
			{
				base.SetValue(row, stringColumn, value, delaySetters, pkColumn);
			}
			else if (column is SchemaBoolColumn booleanColumn)
			{
				SetValue(row, booleanColumn, (ZBool)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaDecimalColumn decimalColumn)
			{
				SetValue(row, decimalColumn, (ZDecimal)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaDateColumn dateColumn)
			{
				SetValue(row, dateColumn, new ZDate(zValue), delaySetters, pkColumn);
			}
			else if (column is SchemaDateTimeColumn dateTimeColumn)
			{
				SetValue(row, dateTimeColumn, (ZDateTime)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaIntColumn intColumn)
			{
				SetValue(row, intColumn, (ZInt)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaShortColumn shortColumn)
			{
				SetValue(row, shortColumn, (ZShort)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaLongColumn longColumn)
			{
				SetValue(row, longColumn, (ZLong)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaGuidColumn guidColumn)
			{
				SetValue(row, guidColumn, new ZGuid(zValue), delaySetters, pkColumn);
			}
			else if (column is SchemaByteColumn byteColumn)
			{
				SetValue(row, byteColumn, (ZByte)zValue, delaySetters, pkColumn);
			}
			else if (column is SchemaBinaryColumn binaryColumn)
			{
				SetValue(row, binaryColumn, (ZBlob)zValue, delaySetters, pkColumn);
			}
		}

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, helper.Factory);
		}
	}

	public class AddInfoDataObjectReader : CommonAddInfoDataObjectReader
	{
		public static AddInfoDataObjectReader New(BusinessObject bizObj, IXmlImportLogger logger, UniversalCommonReaderHelper helper, SchemaStringColumn addInfoColumn, string[] addInfosThatShouldNotBeImported = null, Dictionary<string, SchemaColumn> addInfoColumnsToDBColumnsMapping = null)
		{
			var addInfoSchema = (bizObj as IAddInfoManagerWithSchema)?.AddInfoSchema;
			if (addInfoSchema == null)
			{
				return new AddInfoDataObjectReader(logger, helper, addInfoColumn, addInfosThatShouldNotBeImported, addInfoColumnsToDBColumnsMapping);
			}
			else
			{
				return new BusinessObjectAddInfoDataObjectReader(bizObj.GetType(), logger, helper, addInfoColumn, addInfoSchema, addInfosThatShouldNotBeImported, addInfoColumnsToDBColumnsMapping);
			}
		}

		public AddInfoDataObjectReader(IXmlImportLogger logger, UniversalCommonReaderHelper helper, SchemaStringColumn addInfoColumn, string[] addInfosThatShouldNotBeImported = null, Dictionary<string, SchemaColumn> addInfoColumnsToDBColumnsMapping = null)
			: base(logger, helper, addInfosThatShouldNotBeImported)
		{
			this.addInfoColumn = Argument.NotNull(addInfoColumn, nameof(addInfoColumn));
			this.addInfoColumnsToDBColumnsMapping = addInfoColumnsToDBColumnsMapping;
		}
		protected readonly SchemaStringColumn addInfoColumn;
		protected Dictionary<string, SchemaColumn> addInfoColumnsToDBColumnsMapping;

		public void ReadIntoRow(IAddInfoManager addInfoManager, IColumnIndexer row, IAddInfoCollectionParent addInfoContainer, Dictionary<string, ValueSetter> delaySetters, IOrganizationAddressCollectionParent organizationAddressContainer = null)
		{
			var addInfoCollection = GetApplicableAddInfos(addInfoContainer.AddInfoCollection).ToArray();
			if (addInfoCollection.Length > 0 || organizationAddressContainer != null)
			{
				var infoMappings = GetInfoMappings(addInfoManager);
				ReadCore(addInfoManager, row, addInfoCollection, organizationAddressContainer, infoMappings, delaySetters);
				if (addInfoManager is IAddInfoChildSupporter supporter)
				{
					var reader = new AddInfoChildDataObjectReader(logger, helper);
					reader.ReadIntoBusinessObject(supporter, GetApplicableChildAddInfos(infoMappings, addInfoCollection), delaySetters, organizationAddressContainer);
				}
				if (addInfoManager is IAddInfoWithSyncPropertySupporter addInfoWithSyncPropertySupporter)
				{
					ReadAddInfoSyncProperties(addInfoWithSyncPropertySupporter, row, addInfoCollection, delaySetters, organizationAddressContainer);
				}
			}
		}

		void ReadAddInfoSyncProperties(IAddInfoWithSyncPropertySupporter addInfoWithSyncPropertySupporter, IColumnIndexer row, AddInfo[] addInfoCollection, Dictionary<string, ValueSetter> delaySetters, IOrganizationAddressCollectionParent organizationAddressContainer = null)
		{
			var dictionary = addInfoCollection.ToDictionary(x => x.Key.GetValueOrDefault().ToUpperInvariant(), y => y.Value);
			var pk = addInfoWithSyncPropertySupporter.PK;
			var isDefaultingEnabled = IsDefaultingEnabled;
			foreach ((string addInfoName, _, ZPropertyInfo info, _) in addInfoWithSyncPropertySupporter.GetSyncAddInfos())
			{
				var key = addInfoName.ToUpperInvariant();
				if (dictionary.TryGetValue(key, out var value))
				{
					try
					{
						var zValue = BaseAddInfo.ConvertToZType(info.DefaultValue.GetType(), value.GetValueOrDefault());
						var rowToSet = row;
						if (!isDefaultingEnabled)
						{
							var wrappedPropertyInfo = info as ZWrappedPropertyInfo;
							var infoToCheck = wrappedPropertyInfo?.InnerInfo ?? info;
							var bizObj = infoToCheck.BizObj;
							rowToSet = GetColumnIndexer(bizObj);
						}

						if (delaySetters == null)
						{
							rowToSet[info.Name] = zValue;
						}
						else
						{
							delaySetters.Add(new SyncAddInfoValueSetter(rowToSet, pk, info.Name, () => zValue, logger));
						}
					}
					catch (ZTypeValueException ex)
					{
						logger.Log(Integration.LogType.Error, string.Format("PropertyName: {0}, Value: {1} - {2}", info.Name, value, ex.Message));
					}
				}
			}
		}

		class SyncAddInfoValueSetter : ValueSetter
		{
			public SyncAddInfoValueSetter(IColumnIndexer row, ZGuid pk, ZString propertyName, Func<object> getValue, IXmlImportLogger logger) : base(getValue, logger)
			{
				this.propertyName = propertyName;
				this.row = row;
				this.pk = pk;
			}

			protected override void SetValueCore()
			{
				row[propertyName] = value;
			}

			protected override ZString MatchingKeyCore => pk.ToStringKey() + propertyName;

			readonly ZString propertyName;
			readonly IColumnIndexer row;
			readonly ZGuid pk;
		}

		IEnumerable<AddInfo> GetApplicableChildAddInfos(IDictionary<ZString, AddInfoPropertyNameAndValueParser> mappings, IEnumerable<AddInfo> addInfos)
		{
			foreach (var addInfo in addInfos.Where(x => !mappings.ContainsKey(x.Key.GetValueOrDefault())))
			{
				yield return addInfo;
			}
		}

		protected virtual void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, ValueSetter> delaySetters)
		{
			if (IsDefaultingEnabled && addInfoManager != null)
			{
				ReadCollection(addInfoManager, addInfoCollection, organizationAddresContainer, infoMappings, (key, value, propertyName) =>
				{
					try
					{
						SetValue(addInfoManager, row, key, value, propertyName, delaySetters);
					}
					catch (ZTypeValueException ex)
					{
						logger.Log(Integration.LogType.Error, string.Format("PropertyName: {0}, Value: {1} - {2}", propertyName, value, ex.Message));
					}
				});
			}
			else
			{
				ReadRaw(addInfoManager, row, addInfoCollection, organizationAddresContainer, infoMappings);
			}
		}

		void ReadRaw(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings)
		{
			var addInfos = row.GetAddInfos(addInfoColumn);
			ReadCollection(addInfoManager, addInfoCollection, organizationAddresContainer, infoMappings, (key, value, propertyName) => addInfos.Update(key, value));
			SetValue(row, addInfoColumn, AddInfoParser.Serialise(addInfos));
		}

		void ReadCollection(IAddInfoManager addInfoManager, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Action<ZString, ZString, string> setValue)
		{
			if (addInfoCollection != null)
			{
				ReadAddInfoCollection(addInfoManager, addInfoCollection, infoMappings, setValue);
			}
			if (organizationAddresContainer != null)
			{
				ReadOrganizationAddressCollection(addInfoManager, organizationAddresContainer, setValue);
			}
		}

		protected virtual void ReadOrganizationAddressCollection(IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
		}

		void ReadAddInfoCollection(IAddInfoManager addInfoManager, IEnumerable<AddInfo> addInfoCollection, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Action<ZString, ZString, string> setValue)
		{
			foreach (var addInfo in addInfoCollection)
			{
				if (addInfo != null)
				{
					var key = addInfo.Key.GetValueOrDefault();
					if (!key.IsEmpty)
					{
						var value = addInfo.Value.GetValueOrDefault();
						if (addInfoManager != null && (addInfoColumnsToDBColumnsMapping == null || !addInfoColumnsToDBColumnsMapping.ContainsKey(key)))
						{
							ZString validValue;
							string propertyName;
							if (TryGetValidValue(infoMappings, key, value, logger, out validValue, out propertyName))
							{
								setValue(key, validValue, propertyName);
							}
						}
						else
						{
							setValue(key, value, null);
						}
					}
				}
			}
		}

		protected virtual void SetValue(IAddInfoManager addInfoManager, IColumnIndexer row, ZString key, ZString value, string propertyName, Dictionary<string, ValueSetter> delaySetters)
		{
			if (delaySetters == null)
			{
				addInfoManager.AddInfo.SetValue(propertyName, value);
			}
			else
			{
				delaySetters.Add(new AddInfoValueSetter(row, addInfoColumn, addInfoManager.AddInfo, propertyName, () => value, logger));
			}
		}

		bool TryGetValidValue(IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, ZString key, ZString value, IXmlImportLogger logger, out ZString validValue, out string propertyName)
		{
			var result = false;
			if (infoMappings.TryGetValue(key, out AddInfoPropertyNameAndValueParser data))
			{
				result = true;
				propertyName = data.PropertyName;
				var function = data.Parser;
				validValue = function == null ? value : function(logger, value);
			}
			else
			{
				propertyName = null;
				validValue = ZString.Empty;
			}
			return result;
		}

		protected virtual IDictionary<ZString, AddInfoPropertyNameAndValueParser> GetInfoMappings(IAddInfoManager addInfoManager)
		{
			var addInfo = addInfoManager.AddInfo;
			return addInfo.GetKeys();
		}

		public void AfterUpdateRelatedPropertyCompleted(IAddInfoManager addInfoManager)
		{
			AfterUpdateRelatedPropertyCompletedCore(addInfoManager);
		}

		protected virtual void AfterUpdateRelatedPropertyCompletedCore(IAddInfoManager addInfoManager)
		{
		}
	}

	public class AddInfoValueSetter : ValueSetter
	{
		public AddInfoValueSetter(IColumnIndexer row, SchemaStringColumn column, IAddInfo addInfo, ZString propertyName, Func<object> getValue, IXmlImportLogger logger) : base(getValue, logger)
		{
			this.addInfo = addInfo;
			this.propertyName = propertyName;
			this.row = row;
			this.column = column;
		}

		protected override void SetValueCore()
		{
			addInfo.SetValue(propertyName, (ZString)value);
		}

		protected override ZString MatchingKeyCore => row.GetValue(column.TableSchema.PK).ToStringKey() + propertyName;

		readonly IAddInfo addInfo;
		readonly ZString propertyName;
		readonly IColumnIndexer row;
		readonly SchemaStringColumn column;
	}
}
