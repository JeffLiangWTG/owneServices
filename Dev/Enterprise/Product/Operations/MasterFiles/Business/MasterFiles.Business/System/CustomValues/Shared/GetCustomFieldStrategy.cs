using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GetCustomFieldStrategy : IGetCustomFieldStrategy
	{
		public GetCustomFieldStrategy(BusinessObject parent)
		{
			this.parent = parent;
		}

		readonly BusinessObject parent;

		public IZType GetCustomField(string fieldName, string type = null)
		{
			return GetCustomField(fieldName, out CustomBusinessObject _, type);
		}

		public string GetCustomFieldCodeDescription(string fieldName, string type = null)
		{
			var result = GetCustomField(fieldName, out CustomBusinessObject customBusinessObject, type).ToString();

			if (!string.IsNullOrEmpty(result) && customBusinessObject != null)
			{
				var (property, _) = customBusinessObject.GetPropertyByDescription(fieldName, type);
				var listSourceMetaData = property?.GetMetaData(MetaDataTypes.ListDataSource);
				var codeDescriptionPairList = listSourceMetaData?.Value as CodeDescriptionPairList;
				if (codeDescriptionPairList != null)
				{
					result = codeDescriptionPairList.GetDescriptionFromCode(result);
				}
			}

			return result;
		}

		internal ICustomProperty GetCustomProperty(string propertyDescription, string type, out CustomBusinessObject customBusinessObject)
		{
			if (TryGetCustomBusinessObject(out customBusinessObject))
			{
				var (property, propertyIdentifier) = customBusinessObject.GetPropertyByNameDescription(propertyDescription, type);
				if (property != null && !string.IsNullOrEmpty(propertyIdentifier))
				{
					return customBusinessObject.FindPropertyByIdentifier(propertyIdentifier);
				}
			}
			return null;
		}

		IZType GetCustomField(string fieldName, out CustomBusinessObject customBusinessObject, string type = null)
		{
			if (TryGetCustomBusinessObject(out customBusinessObject))
			{
				var accessor = customBusinessObject.GetCustomFieldAccessor(fieldName, type);
				return accessor?.GetValue() ?? UniversalNullObject.Instance;
			}

			return UniversalNullObject.Instance;
		}

		bool TryGetCustomBusinessObject(out CustomBusinessObject customBusinessObject)
		{
			if (parent is ICustomFieldProvider customFieldProvider)
			{
				customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			}
			else
			{
				var customProperties = new UserDefinedPropertyCollection(parent);
				customProperties.LoadAllPresavedPropertiesOnParent();
				customBusinessObject = new CustomBusinessObject(parent.Factory, parent, customProperties);
			}

			return customBusinessObject != null;
		}
	}
}
