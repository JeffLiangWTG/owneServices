using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class DynamicBusinessObjectExtensionsInstance : IDynamicBusinessObjectExtensionsInstance
	{
		public (DynamicBusinessObjectProperty, string) GetPropertyByDescription(IDynamicBusinessObject dynamicBusinessObject, string fieldName, string type)
		{
			return dynamicBusinessObject.GetPropertyByDescription(fieldName, type);
		}

		public (string, string) DescriptionsFor(DynamicBusinessObjectProperty property)
		{
			return IDynamicBusinessObjectExtensions.DescriptionsFor(property);
		}
	}

	public static class IDynamicBusinessObjectExtensions
	{
		public static (DynamicBusinessObjectProperty, string) GetPropertyByNameDescription(this IDynamicBusinessObject dynamicBusinessObject, string fieldName, string type)
		{
			var (metaData, propertyName) = GetPropertyByName(dynamicBusinessObject, fieldName, type);
			if (metaData == null || string.IsNullOrEmpty(propertyName))
			{
				(metaData, propertyName) = GetPropertyByDescription(dynamicBusinessObject, fieldName, type);
			}
			return (metaData, propertyName);
		}

		#region GetPropertyByDescription

		public static (DynamicBusinessObjectProperty, string) GetPropertyByDescription(this IDynamicBusinessObject dynamicBusinessObject, string fieldName, string type)
		{
			foreach (var propertyName in dynamicBusinessObject.PropertyNames)
			{
				var property = dynamicBusinessObject.GetProperty(propertyName);
				if (property != null)
				{
					var (description, partdescription) = DescriptionsFor(property);
					if (description != null)
					{
						Type realType = null;
						try
						{
							realType = string.IsNullOrEmpty(type) ? null : AddOnColumnDataType.GetTypeFromCode(type);
						}
						catch (ArgumentException)
						{
							// Invalid type.
						}

						if ((fieldName.Equals(description, StringComparison.OrdinalIgnoreCase) || fieldName.Equals(partdescription, StringComparison.OrdinalIgnoreCase))
							&& (realType == null || property.Type == realType))
						{
							return (property, propertyName);
						}
					}
				}
			}

			return (null, string.Empty);
		}

		public static (string, string) DescriptionsFor(DynamicBusinessObjectProperty property)
		{
			var metaData = property.GetMetaData(MetaDataTypes.Description);
			var descriptionObject = metaData != null ? metaData.Value as IDescription : null;
			if (descriptionObject != null)
			{
				var description = descriptionObject.GetDescription(0, CultureInfo.InvariantCulture);
				var partdescription = description;
				if ((int)(property.GetMetaData(MetaDataTypes.CustomFieldPosition)?.Value ?? -1) > 0)
				{
					partdescription += AddOnColumnDataType.PartIdentifier + property.GetMetaData(MetaDataTypes.CustomFieldPosition).Value;
				}
				return (description, partdescription);
			}
			return (null, null);
		}

		#endregion

		#region GetPropertyByName

		public static (DynamicBusinessObjectProperty, string) GetPropertyByName(this IDynamicBusinessObject dynamicBusinessObject, string fieldName, string type)
		{
			foreach (var propertyName in dynamicBusinessObject.PropertyNames)
			{
				var property = dynamicBusinessObject.GetProperty(propertyName);
				if (property != null)
				{
					var name = NameFor(property);
					if (name != null)
					{
						Type realType = null;
						try
						{
							realType = string.IsNullOrEmpty(type) ? null : AddOnColumnDataType.GetTypeFromCode(type);
						}
						catch (ArgumentException)
						{
							// Invalid type.
						}

						if ((fieldName.Equals(name, StringComparison.OrdinalIgnoreCase))
							&& (realType == null || property.Type == realType))
						{
							return (property, propertyName);
						}
					}
				}
			}

			return (null, string.Empty);
		}

		public static string NameFor(DynamicBusinessObjectProperty property)
		{
			var metaData = property.GetMetaData(MetaDataTypes.Name);
			return metaData != null ? metaData.Value as string : null;
		}

		#endregion
	}
}
