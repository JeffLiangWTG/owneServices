using System;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class AddInfoDescAttribute : Attribute
	{
		public AddInfoDescAttribute(string propertyName)
		{
			this.propertyName = propertyName;
		}

		public string PropertyName
		{
			get { return propertyName; }
		}
		readonly string propertyName;

		public ZString GetDescription(object parentObject)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(PropertyName))
			{
				var propertyInfo = parentObject.GetType().GetProperty(PropertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
					?? throw new ArgumentException("Can not find the match property.", PropertyName);
				result = (ZString)propertyInfo.GetValue(parentObject);
			}
			return result;
		}
	}
}
