using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public static class CustomLabelsCustomizedFieldDataObjectWriter
	{
		public static void Write(ITableSchema tableSchema, IColumnIndexer row, ICustomizedFieldContainer customizedFieldsContainer, ICustomLabelsProvider provider)
		{
			Write((r, s) =>
			{
				var column = tableSchema.All[s];
				return column != null ? r.GetValue(column) : null;
			}, row, customizedFieldsContainer, provider);
		}

		public static void Write(Func<IColumnIndexer, string, IZType> getValue, IColumnIndexer row, ICustomizedFieldContainer customizedFieldsContainer, ICustomLabelsProvider provider)
		{
			if (provider != null && getValue != null)
			{
				var fieldList = provider.GetCustomFields(provider.ConfigOrgProvider.ConfigOrg, provider.ConfigOrgProvider.Factory);
				if (fieldList.Count > 0)
				{
					var customizedFieldCollection = customizedFieldsContainer.CustomizedFieldCollection ?? new List<CustomizedField>();
					foreach (CustomLabelInfoBase shipmentCustomField in fieldList)
					{
						if (shipmentCustomField.IsEnabled)
						{
							var value = getValue(row, shipmentCustomField.PropertyName);
							if (value != null)
							{
								customizedFieldCollection.Add(CustomizedField.New(shipmentCustomField.Caption.GetUnresolvedString(), value));
							}
						}
					}
					customizedFieldsContainer.SetCustomizedFieldCollection(() => customizedFieldCollection.Count > 0 ? customizedFieldCollection : null);
				}
			}
		}
	}
}
