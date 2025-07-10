using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CustomFieldsDataObjectWritingHelper<T>
		where T : BusinessObject
	{
		public CustomFieldsDataObjectWritingHelper(T customFieldsParent, CustomFieldsDescriptor<T> customFieldsDescriptor)
		{
			this.customFieldsParent = Argument.NotNull(customFieldsParent, "customFieldsParent");
			this.customFieldsDescriptor = Argument.NotNull(customFieldsDescriptor, "customFieldsDescriptor");
		}

		readonly T customFieldsParent;
		readonly CustomFieldsDescriptor<T> customFieldsDescriptor;

		public IEnumerable<IPropertyValue> GetUserDefinedValues()
		{
			return GetValues((key, value) => new PropertyValue(key, value));
		}

		public IEnumerable<CustomizedField> GetCustomizedFieldValues()
		{
			return GetValues((key, value) => CustomizedField.New(key, value));
		}

		IEnumerable<TVal> GetValues<TVal>(Func<string, IZType, TVal> valueCreator)
		{
			return customFieldsDescriptor
				.ActiveCustomFieldsInfos
				.Select(customFieldInfo =>
				{
					var name = customFieldInfo.Caption;
					var value = (IZType)customFieldsParent[customFieldInfo.SchemaColumnName];
					return valueCreator(name, value);
				});
		}
	}
}
