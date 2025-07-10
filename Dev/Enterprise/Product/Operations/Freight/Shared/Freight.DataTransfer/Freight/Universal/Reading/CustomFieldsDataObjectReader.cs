using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CustomFieldsDataObjectReader<T> : DataObjectReader
		where T : BusinessObject
	{
		public CustomFieldsDataObjectReader(IXmlImportLogger logger, T customFieldsParent, CustomFieldsDescriptor<T> customFieldsDescriptor)
			: base(logger)
		{
			this.customFieldsParent = Argument.NotNull(customFieldsParent, "customFieldsParent");
			this.customFieldsDescriptor = Argument.NotNull(customFieldsDescriptor, "customFieldsDescriptor");
		}

		readonly T customFieldsParent;
		readonly CustomFieldsDescriptor<T> customFieldsDescriptor;

		public (string, DataType?)[] ReadCustomFields(IEnumerable<CustomizedField> customFieldDataCollection)
		{
			if ((!customFieldDataCollection?.Any()) ?? false)
			{
				return null;
			}

			var usedCustomFieldDataCollection = new List<(string, DataType?)>();
			var customFieldsDataByColumnName = customFieldDataCollection
				.GroupBy(customFieldData => (name: customFieldData.Key.GetValueOrDefault(), type: customFieldData.DataType))
				.Where(group => !group.Key.name.IsEmpty);

			var dataTypeConverter = new DataTypeConverter();
			foreach (var customFieldData in customFieldsDataByColumnName)
			{
				var descriptors = customFieldsDescriptor
					.CustomFieldsInfos
					.Where(info =>
						!string.IsNullOrWhiteSpace(info.Caption)
						&& info.Caption.Equals(customFieldData.Key.name, StringComparison.InvariantCultureIgnoreCase)).ToList();
				if (descriptors.Any())
				{
					var matchingCustomFieldInfo = descriptors.FirstOrDefault(info => dataTypeConverter.ToEnumValue(info.ZDataType) == customFieldData.Key.type)
						?? descriptors.First();

					if (customFieldData.Count() > 1)
					{
						logger.Log(LogType.Warning, Res.GetString("863585a2-af01-42bf-9cad-1c8660f5f328", "Element {0} has duplicates. Only the value from the first element one will be used.", matchingCustomFieldInfo.Caption));
					}

					var schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(matchingCustomFieldInfo.SchemaColumnName, customFieldsParent.TableName);

					if (schemaColumn != null)
					{
						var value = GetValue(customFieldData.First(), schemaColumn.GetEquivalentZType());
						if (value != null)
						{
							SetValue(customFieldsParent, schemaColumn, value);
						}
						usedCustomFieldDataCollection.Add((customFieldData.Key.name.ToLower(), customFieldData.Key.type));
					}
				}
			}

			return usedCustomFieldDataCollection.ToArray();
		}
	}
}
