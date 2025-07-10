using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public static class BusinessObjectExtensionMethods
	{
		public static int GetDecimalPlacesMetaData(this BusinessObject bizObj, string propertyName)
		{
			int result = -1;

			var bizObjType = bizObj.GetType();

			while (bizObjType != typeof(BusinessObject))
			{
				var propertyInfo = bizObjType.GetProperty(propertyName);

				if (propertyInfo != null)
				{
					var attributes = propertyInfo.GetCustomAttributes(typeof(DecimalPlacesAttribute), true);
					if (attributes.Length > 0)
					{
						result = ((DecimalPlacesAttribute)attributes[0]).DecimalPlaces;
						break;
					}
				}

				bizObjType = bizObjType.BaseType;
			}

			return result;
		}
	}
}
