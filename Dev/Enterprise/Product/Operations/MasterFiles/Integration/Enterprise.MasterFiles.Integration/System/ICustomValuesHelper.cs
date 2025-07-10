using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICustomValuesHelper
	{
		IEnumerable<IPropertyValue> GetUserDefinedValues(BusinessObject businessObject);
		void SetUserDefinedValue(BusinessObject businessObject, string propertyName, IZType value, INotifications notifications = null);
		void SetUserDefinedValue(BusinessObject businessObject, string propertyName, string propertyType, IZType value, INotifications notifications = null);
	}
}
