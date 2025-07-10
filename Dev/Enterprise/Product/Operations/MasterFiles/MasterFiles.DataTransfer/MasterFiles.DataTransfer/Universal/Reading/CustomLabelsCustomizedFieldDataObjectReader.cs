using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class CustomLabelsCustomizedFieldDataObjectReader : DataObjectReader
	{
		public CustomLabelsCustomizedFieldDataObjectReader(IXmlImportLogger logger)
			: base(logger)
		{
		}

		public (string, DataType?)[] PopulateCustomFields(ITableSchema tableSchema, IColumnIndexer row, ICustomizedFieldContainer customizedFieldsContainer, ICustomLabelsProvider provider, Dictionary<string, ValueSetter> delaySetters = null)
		{
			var usedCustomFields = new List<(string, DataType?)>();

			var customizedFields = customizedFieldsContainer.CustomizedFieldCollection;
			if (customizedFields != null)
			{
				var fieldList = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, provider.ConfigOrgProvider.Factory);
				if (fieldList.Count > 0)
				{
					var customFieldsDictionary = new Dictionary<string, CustomLabelInfoBase>();
					foreach (CustomLabelInfoBase shipmentCustomField in fieldList)
					{
						if (shipmentCustomField.IsEnabled)
						{
							var key = shipmentCustomField.Caption.ToString().ToUpper();
							if (!customFieldsDictionary.ContainsKey(key))
							{
								customFieldsDictionary.Add(key, shipmentCustomField);
							}
						}
					}

					foreach (var customizedField in customizedFields)
					{
						var customLabelInfo = GetMatchCustomLabelInfo(customFieldsDictionary, customizedField);
						if (customLabelInfo != null)
						{
							usedCustomFields.Add((customizedField.Key.Value.ToLower(), customizedField.DataType));
							var column = tableSchema.All[customLabelInfo.PropertyName];
							if (column != null)
							{
								var value = GetValue(customizedField);
								if (value != null)
								{
									SetValueUsingDelaySetterIfNeeded(row, column, value, delaySetters);
								}
							}
						}
					}
				}
			}
			return usedCustomFields.ToArray();
		}

		void SetValueUsingDelaySetterIfNeeded(IColumnIndexer row, SchemaColumn column, IZType value, Dictionary<string, ValueSetter> delaySetters)
		{
			switch (column.ColumnType)
			{
				case SchemaColumnType.String:
					SetValue(row, (SchemaStringColumn)column, (ZString)value, delaySetters);
					break;

				case SchemaColumnType.Guid:
					SetValue(row, (SchemaGuidColumn)column, (ZGuid)value, delaySetters);
					break;

				case SchemaColumnType.Date:
				case SchemaColumnType.DateTime:
					SetValue(row, (SchemaDateTimeColumn)column, (ZDateTime)value, delaySetters);
					break;

				case SchemaColumnType.Int:
				case SchemaColumnType.Short:
					if (value.GetType() == typeof(ZShort))
					{
						SetValue(row, (SchemaShortColumn)column, (ZShort)value, delaySetters);
					}
					else
					{
						SetValue(row, (SchemaIntColumn)column, (ZInt)value, delaySetters);
					}
					break;

				case SchemaColumnType.Byte:
					SetValue(row, (SchemaByteColumn)column, (ZByte)value, delaySetters);
					break;

				case SchemaColumnType.Blob:
				case SchemaColumnType.Binary:
					SetValue(row, (SchemaBinaryColumn)column, (ZBlob)value, delaySetters);
					break;

				case SchemaColumnType.Bool:
					SetValue(row, (SchemaBoolColumn)column, (ZBool)value, delaySetters);
					break;

				case SchemaColumnType.Decimal:
					SetValue(row, (SchemaDecimalColumn)column, (ZDecimal)value, delaySetters);
					break;

				default:
					row.SetValue(column, value);
					ErrorReporter.ReportOnce("Column type not supported in CustomLabelsCustomizedFieldDataObjectReader" + column.GetType());
					break;
			}
		}

		static CustomLabelInfoBase GetMatchCustomLabelInfo(Dictionary<string, CustomLabelInfoBase> customFieldsDictionary, CustomizedField customField)
		{
			CustomLabelInfoBase result;
			customFieldsDictionary.TryGetValue(customField.Key.GetValueOrDefault().ToUpper(), out result);
			return result != null && new DataTypeConverter().ToEnumValue(result.PropertyType) == customField.DataType ? result : null;
		}
	}
}
