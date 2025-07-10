using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class CustomValuesHelper : ICustomValuesHelper
	{
		public IEnumerable<IPropertyValue> GetUserDefinedValues(BusinessObject businessObject)
		{
			return businessObject.GetUserDefinedValues();
		}

		public void SetUserDefinedValue(BusinessObject businessObject, string propertyName, IZType value, INotifications notifications = null)
		{
			businessObject.SetUserDefinedValue(propertyName, null, value, notifications);
		}

		public void SetUserDefinedValue(BusinessObject businessObject, string propertyName, string propertyType, IZType value, INotifications notifications = null)
		{
			if (propertyType == null)
			{
				propertyType = AddOnColumnDataType.GetCodeFromObject(value);
			}
			businessObject.SetUserDefinedValue(propertyName, propertyType, value, notifications);
		}
	}
}
