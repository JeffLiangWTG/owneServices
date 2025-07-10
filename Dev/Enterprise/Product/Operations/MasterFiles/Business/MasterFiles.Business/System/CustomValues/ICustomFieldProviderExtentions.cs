using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public static class ICustomFieldProviderExtensions
	{
		public static IZType GetCustomField(this ICustomFieldProvider customFieldProvider, string fieldName, string type)
		{
			var accessor = GetCustomFieldAccessor(customFieldProvider, fieldName, type);

			if (accessor != null)
			{
				return accessor.GetValue();
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static CustomFieldAccessor GetCustomFieldAccessor(this ICustomFieldProvider customFieldProvider, string fieldName, string type)
		{
			if (!string.IsNullOrEmpty(fieldName))
			{
				var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
				if (customBusinessObject == null)
				{
					return null;
				}

				var (metaData, propertyName) = customBusinessObject.GetPropertyByNameDescription(fieldName, type);
				if (metaData != null && !string.IsNullOrEmpty(propertyName))
				{
					return new CustomFieldAccessor(customBusinessObject, metaData, propertyName);
				}
			}

			return null;
		}

		public class CustomFieldAccessor
		{
			internal CustomFieldAccessor(CustomBusinessObject customObject, DynamicBusinessObjectProperty metaData, string propertyName)
			{
				CustomObject = customObject;
				MetaData = metaData;
				PropertyName = propertyName;
			}

			public IZType GetValue()
			{
				var customValue = CustomObject[PropertyName];
				return customValue is IZType zVal ? zVal : (ZString)customValue.ToString();
			}

			public void SetValue(IZType value)
			{
				CustomObject[PropertyName] = value;
			}

			public CustomBusinessObject CustomObject { get; }
			public DynamicBusinessObjectProperty MetaData { get; }
			public string PropertyName { get; }
		}
	}
}
